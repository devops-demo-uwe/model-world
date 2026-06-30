using System.Reflection;
using Microsoft.Extensions.Configuration;
using ModelWorld.Catalogs;
using ModelWorld.Console;
using ModelWorld.Models;
using ModelWorld.Services;

var isDemo = args.Any(argument => string.Equals(argument, "--demo", StringComparison.OrdinalIgnoreCase));
var shouldShowHelp = args.Any(argument => string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase));
var isStaticMode = args.Any(argument => string.Equals(argument, "--static", StringComparison.OrdinalIgnoreCase));
var isLiveMode = !isStaticMode;
var demoPromptId = GetOptionValue(args, "--prompt") ?? "math-check";
var models = isLiveMode ? ModelCatalog.Live : ModelCatalog.All;
var prompts = PromptCatalog.All;
var renderer = new ConsoleRenderer();
IReadOnlyDictionary<string, ModelPricing>? pricingByModelId = null;

if (shouldShowHelp)
{
	renderer.RenderIntro(isLiveMode);
	renderer.RenderHelpSection(isLiveMode);
	return;
}

var runner = await CreateRunnerAsync();

if (runner is null)
{
	Environment.ExitCode = 1;
	return;
}

if (isDemo)
{
	renderer.RenderIntro(isLiveMode);
	renderer.RenderModelTable(models, pricingByModelId);

	var demoModels = isLiveMode
		? ModelCatalog.GetDefaultLiveComparisonModels()
		: ModelCatalog.GetDefaultComparisonModels();
	await RunComparisonAsync(demoModels, [PromptCatalog.GetById(demoPromptId)]);
	return;
}

while (true)
{
	renderer.RenderIntro(isLiveMode);
	renderer.RenderModelTable(models, pricingByModelId);

	var selectedAction = renderer.SelectMainMenuAction();
	if (selectedAction == MainMenuAction.Exit)
	{
		break;
	}

	if (selectedAction == MainMenuAction.ViewHelp)
	{
		renderer.RenderHelpSection(isLiveMode);
		renderer.WaitForHelpReturn();
		continue;
	}

	if (selectedAction == MainMenuAction.ViewEnterpriseCostExample)
	{
		renderer.RenderEnterpriseChatCostExample(models, EnterpriseChatUsageProfile.MediumCorporate, pricingByModelId);
		renderer.WaitForMainMenuReturn();
		continue;
	}

	var selectedModels = renderer.SelectModels(models);
	var selectedPrompts = renderer.SelectPrompts(prompts);

	await RunComparisonAsync(selectedModels, selectedPrompts);

	if (!renderer.ShouldRunAnotherComparison())
	{
		break;
	}
}

renderer.RenderGoodbye();

async Task RunComparisonAsync(
	IReadOnlyList<ModelWorld.Models.ModelProfile> selectedModels,
	IReadOnlyList<ModelWorld.Models.PromptScenario> selectedPrompts)
{
	renderer.RenderPromptTable(selectedPrompts);

	IReadOnlyList<ModelWorld.Models.SimulationResult> results = [];
	if (!isLiveMode)
	{
		await renderer.ShowProgressAsync(async () =>
		{
			results = await runner.RunAsync(selectedModels, selectedPrompts);
		}, "Running static simulation...");
	}
	else
	{
		await renderer.ShowProgressMarkupAsync(async updateStatus =>
		{
			List<ModelWorld.Models.SimulationResult> liveResults = [];
			foreach (var prompt in selectedPrompts)
			{
				foreach (var model in selectedModels)
				{
					updateStatus(ModelWorld.Console.ConsoleRenderer.BuildModelPromptStatusMarkup(model.DisplayName, prompt.Title));
					liveResults.AddRange(await runner.RunAsync([model], [prompt]));
				}
			}

			results = liveResults;
		}, ModelWorld.Console.ConsoleRenderer.BuildProgressStatusMarkup("Running live Azure AI Foundry requests..."));
	}

	renderer.RenderResults(results);
}

async Task<IModelRunner?> CreateRunnerAsync()
{
	if (!isLiveMode)
	{
		return new StaticModelSimulator();
	}

	try
	{
		var configuration = new ConfigurationBuilder()
			.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
			.AddEnvironmentVariables()
			.Build();
		var options = configuration.GetSection(AzureFoundryOptions.SectionName).Get<AzureFoundryOptions>() ?? new AzureFoundryOptions();
		if (isDemo && options.RequestTimeoutSeconds == AzureFoundryOptions.DefaultRequestTimeoutSeconds)
		{
			options.RequestTimeoutSeconds = 45;
		}

		options.GetNormalizedEndpoint();
		options.GetMaxOutputTokenCount();
		options.GetTemperature();
		options.GetRequestTimeout();
		var pricingEndpoint = options.GetPricingEndpoint();

		pricingByModelId = await LoadPricingAsync(models, pricingEndpoint);

		return new AzureModelRunner(options, pricingByModelId);
	}
	catch (InvalidOperationException exception)
	{
		renderer.RenderConfigurationError(exception.Message);
		return null;
	}
}

async Task<IReadOnlyDictionary<string, ModelPricing>> LoadPricingAsync(
	IReadOnlyList<ModelProfile> pricingModels,
	Uri pricingEndpoint)
{
	try
	{
		using var httpClient = new HttpClient();
		var pricingProvider = new AzureRetailPricesPricingProvider(httpClient, pricingEndpoint);
		return await pricingProvider.GetPricingAsync(pricingModels);
	}
	catch (Exception exception) when (exception is HttpRequestException or NotSupportedException or System.Text.Json.JsonException or InvalidOperationException or TaskCanceledException)
	{
		return pricingModels.ToDictionary(
			model => model.Id,
			model => ModelPricing.CatalogFallback(
				model,
				AzureRetailPricesPricingProvider.CatalogFallbackSourceName,
				GetCatalogPricingRegion(model),
				"catalog fallback: API pricing lookup failed"),
			StringComparer.OrdinalIgnoreCase);
	}
}

static string GetCatalogPricingRegion(ModelProfile model) =>
	string.IsNullOrWhiteSpace(model.PricingRegion)
		? "unknown"
		: model.PricingRegion.Trim().ToLowerInvariant();

static string? GetOptionValue(string[] arguments, string optionName)
{
	for (var index = 0; index < arguments.Length - 1; index++)
	{
		if (string.Equals(arguments[index], optionName, StringComparison.OrdinalIgnoreCase))
		{
			return arguments[index + 1];
		}
	}

	return null;
}
