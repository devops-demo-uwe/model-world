using System.Globalization;
using ModelWorld.Models;
using Spectre.Console;

namespace ModelWorld.Console;

public sealed class ConsoleRenderer
{
    private const int MaximumComparedModels = 3;

    public void RenderIntro()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Model World").Color(Color.Teal));
        AnsiConsole.Write(new Panel(
                "[bold]Static prototype[/]: no Azure calls are made. Model behavior, latency, tokens, and costs are illustrative estimates for exploring the future Foundry flow.")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Header(" Preview "));
        AnsiConsole.WriteLine();
    }

    public void RenderModelTable(IReadOnlyList<ModelProfile> models)
    {
        var table = new Table()
            .Title("[bold teal]Model Catalog[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("Model")
            .AddColumn("Family")
            .AddColumn("Context")
            .AddColumn("Typical Latency")
            .AddColumn("Est. Price / 1M Tokens")
            .AddColumn("Best At");

        foreach (var model in models)
        {
            table.AddRow(
                Markup.Escape(model.DisplayName),
                Markup.Escape(model.Family),
                FormatWholeNumber(model.ContextWindowTokens),
                $"{FormatWholeNumber(model.TypicalLatencyMilliseconds)} ms",
                $"${FormatCurrencyValue(model.InputCostPerMillionTokensUsd)} in / ${FormatCurrencyValue(model.OutputCostPerMillionTokensUsd)} out",
                Markup.Escape(model.RecommendedUseCases));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void RenderPromptTable(IReadOnlyList<PromptScenario> prompts)
    {
        var table = new Table()
            .Title("[bold teal]Prompt Gallery[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("Domain")
            .AddColumn("Title")
            .AddColumn("Reveals");

        foreach (var prompt in prompts)
        {
            table.AddRow(
                Markup.Escape(prompt.Domain),
                Markup.Escape(prompt.Title),
                Markup.Escape(prompt.Reveals));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void RenderResults(IReadOnlyList<SimulationResult> results)
    {
        foreach (var group in results.GroupBy(result => result.Prompt))
        {
            AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(group.Key.Domain)} - {Markup.Escape(group.Key.Title)}[/]").RuleStyle("grey"));
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(group.Key.PromptText)}[/]");
            AnsiConsole.WriteLine();

            RenderPromptComparisonTable(group.ToArray());
        }
    }

    public void RenderRunSummary(IReadOnlyList<SimulationResult> results)
    {
        var table = new Table()
            .Title("[bold teal]Run Summary[/]")
            .Border(TableBorder.Simple)
            .AddColumn("Model")
            .AddColumn("Prompt")
            .AddColumn("Elapsed")
            .AddColumn("Tokens")
            .AddColumn("Est. Cost")
            .AddColumn("Finish");

        foreach (var result in results)
        {
            table.AddRow(
                Markup.Escape(result.Model.DisplayName),
                Markup.Escape(result.Prompt.Title),
                $"{FormatWholeNumber(result.Elapsed.TotalMilliseconds)} ms",
                FormatWholeNumber(result.TotalTokens),
                $"${FormatCost(result.Cost.TotalCostUsd)}",
                Markup.Escape(result.FinishReason));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public IReadOnlyList<ModelProfile> SelectModels(IReadOnlyList<ModelProfile> models)
    {
        var choices = models.Select(model => model.DisplayName).ToArray();

        while (true)
        {
            var selected = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title($"Choose up to {MaximumComparedModels} models to compare")
                    .InstructionsText("[grey](Press <space> to toggle a model, <enter> to run.)[/]")
                    .Required()
                    .PageSize(8)
                    .AddChoices(choices));

            if (selected.Count <= MaximumComparedModels)
            {
                return selected
                    .Select(displayName => models.First(model => model.DisplayName == displayName))
                    .ToArray();
            }

            AnsiConsole.MarkupLine($"[red]Choose {MaximumComparedModels} or fewer models so the comparison fits in columns.[/]");
        }
    }

    public IReadOnlyList<PromptScenario> SelectPrompts(IReadOnlyList<PromptScenario> prompts)
    {
        var choices = new[] { "All prompts" }.Concat(prompts.Select(prompt => $"{prompt.Domain}: {prompt.Title}")).ToArray();
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a prompt")
                .AddChoices(choices));

        return selected == "All prompts"
            ? prompts
            : [prompts.First(prompt => selected.EndsWith(prompt.Title, StringComparison.Ordinal))];
    }

    public bool ShouldRunComparison() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .AddChoices("Run a model comparison", "Exit Model World")) == "Run a model comparison";

    public bool ShouldRunAnotherComparison() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Keep exploring?")
                .AddChoices("Run another comparison", "Exit Model World")) == "Run another comparison";

    public void RenderGoodbye()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Thanks for exploring Model World.[/]");
    }

    public async Task ShowProgressAsync(Func<Task> action)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("teal"))
            .StartAsync("Running static simulation...", async _ =>
            {
                await Task.Delay(250);
                await action();
            });
    }

    private static void RenderPromptComparisonTable(IReadOnlyList<SimulationResult> results)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Expand()
            .AddColumn(new TableColumn("[grey]Metric[/]").NoWrap());

        foreach (var result in results)
        {
            table.AddColumn(new TableColumn($"[bold teal]{Markup.Escape(result.Model.DisplayName)}[/]"));
        }

        table.AddRow(BuildRow("Elapsed", results, result => $"{FormatWholeNumber(result.Elapsed.TotalMilliseconds)} ms"));
        table.AddRow(BuildRow("Tokens", results, result => $"{FormatWholeNumber(result.PromptTokens)} prompt\n{FormatWholeNumber(result.CompletionTokens)} completion\n{FormatWholeNumber(result.TotalTokens)} total"));
        table.AddRow(BuildRow("Estimated cost", results, result => $"${FormatCost(result.Cost.TotalCostUsd)}"));
        table.AddRow(BuildRow("Finish", results, result => Markup.Escape(result.FinishReason)));
        table.AddRow(BuildRow("Note", results, result => Markup.Escape(result.Note ?? "")));
        table.AddRow(BuildRow("Output", results, result => Markup.Escape(result.Output)));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static string[] BuildRow(
        string label,
        IReadOnlyList<SimulationResult> results,
        Func<SimulationResult, string> valueFactory) =>
        [Markup.Escape(label), .. results.Select(valueFactory)];

    private static string FormatWholeNumber(int value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatWholeNumber(double value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatCurrencyValue(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string FormatCost(decimal value) =>
        value.ToString("0.000000", CultureInfo.InvariantCulture);
}