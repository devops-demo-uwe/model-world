using ModelWorld.Catalogs;
using ModelWorld.Console;
using ModelWorld.Services;

var models = ModelCatalog.All;
var prompts = PromptCatalog.All;
var renderer = new ConsoleRenderer();
var runner = new StaticModelSimulator();

var isDemo = args.Any(argument => string.Equals(argument, "--demo", StringComparison.OrdinalIgnoreCase));

if (isDemo)
{
	renderer.RenderIntro();
	renderer.RenderModelTable(models);
	renderer.RenderPromptTable(prompts);

	await RunComparisonAsync(ModelCatalog.GetDefaultComparisonModels(), [PromptCatalog.GetById("math-check")]);
	return;
}

while (true)
{
	renderer.RenderIntro();
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
	});

	renderer.RenderRunSummary(results);
	renderer.RenderResults(results);
}
