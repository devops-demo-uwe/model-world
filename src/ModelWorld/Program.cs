using System.Reflection;
using Microsoft.Extensions.Configuration;
using ModelWorld.Catalogs;
using ModelWorld.Console;
using ModelWorld.Services;

var isDemo = args.Any(argument => string.Equals(argument, "--demo", StringComparison.OrdinalIgnoreCase));
var isStaticMode = args.Any(argument => string.Equals(argument, "--static", StringComparison.OrdinalIgnoreCase));
var isLiveMode = !isStaticMode;
var models = isLiveMode ? ModelCatalog.Live : ModelCatalog.All;
var prompts = PromptCatalog.All;
var renderer = new ConsoleRenderer();
var runner = CreateRunner();

if (runner is null)
{
	Environment.ExitCode = 1;
	return;
}

if (isDemo)
{
	renderer.RenderIntro(isLiveMode);
	renderer.RenderModelTable(models);
	renderer.RenderPromptTable(prompts);

	var demoModels = isLiveMode
		? ModelCatalog.GetDefaultLiveComparisonModels()
		: ModelCatalog.GetDefaultComparisonModels();
	await RunComparisonAsync(demoModels, [PromptCatalog.GetById("math-check")]);
	return;
}

while (true)
{
	renderer.RenderIntro(isLiveMode);
	renderer.RenderModelTable(models);
	renderer.RenderPromptTable(prompts);

	if (!renderer.ShouldRunComparison())
	{
		break;
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
	IReadOnlyList<ModelWorld.Models.SimulationResult> results = [];
	await renderer.ShowProgressAsync(async () =>
	{
		results = await runner.RunAsync(selectedModels, selectedPrompts);
	}, isLiveMode ? "Running live Azure AI Foundry requests..." : "Running static simulation...");

	renderer.RenderRunSummary(results);
	renderer.RenderResults(results);
}

IModelRunner? CreateRunner()
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

		options.GetNormalizedEndpoint();
		options.GetMaxOutputTokenCount();
		options.GetTemperature();
		options.GetRequestTimeout();

		return new AzureModelRunner(options);
	}
	catch (InvalidOperationException exception)
	{
		renderer.RenderConfigurationError(exception.Message);
		return null;
	}
}
