using System.Globalization;
using ModelWorld.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ModelWorld.Console;

public sealed class ConsoleRenderer
{
    private const int LayoutWidth = 120;
    private const int BannerInnerWidth = LayoutWidth - 4;
    private const int MaximumComparedModels = 3;
    private const string Accent = "#38bdf8";
    private const string AccentAlt = "#f472b6";
    private const string Success = "#34d399";
    private const string Warning = "#fbbf24";
    private const string Muted = "#94a3b8";
    private const string NerdSpark = "󰐕";
    private const string NerdModel = "󰚩";
    private const string NerdPrompt = "󰈙";
    private const string NerdRun = "󰐊";
    private const string NerdCost = "󰃭";
    private const string NerdExit = "󰗼";
    private const string NerdAzure = "󰠅";
    private const string NerdChart = "󰄧";
    private const string NerdTimer = "󰔟";
    private const string NerdTokens = "󰓡";

    private static readonly Color AccentColor = new(56, 189, 248);
    private static readonly Color AccentAltColor = new(244, 114, 182);
    private static readonly Color SuccessColor = new(52, 211, 153);
    private static readonly Color WarningColor = new(251, 191, 36);
    private static readonly Color PanelFillColor = new(18, 25, 38);

    public ConsoleRenderer()
    {
        AnsiConsole.Profile.Width = LayoutWidth;
    }

    public void RenderIntro()
    {
        if (!System.Console.IsOutputRedirected)
        {
            AnsiConsole.Clear();
        }

        RenderTitleBanner();

        WriteFullWidth(new Panel(
                $"[bold {Warning}]Static prototype[/]: [white]no Azure calls are made.[/]\n[{Muted}]Model behavior, latency, tokens, and costs are illustrative estimates for exploring the future Foundry flow.[/]")
            .Border(BoxBorder.Double)
            .BorderColor(AccentAltColor)
            .Header($" [bold {AccentAlt}][/][bold black on {AccentAlt}] Preview [/][bold {AccentAlt}][/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();
    }

    private static void RenderTitleBanner()
    {
        var title = new Rows(
        [
            Align.Center(new Markup($"[bold {Accent}]╭─[/][bold {AccentAlt}][/][bold black on {AccentAlt}] {NerdAzure} Azure AI Foundry [/][bold {AccentAlt}][/][bold {Success}][/][bold black on {Success}] {NerdChart} Compare [/][bold {Success}][/][bold {Warning}][/][bold black on {Warning}] {NerdTimer} Measure [/][bold {Warning}][/][bold {Accent}]─╮[/]")).Width(BannerInnerWidth),
            .. BuildLogoWordmark(),
            Align.Center(new Markup($"[bold {Accent}]╰─[/][bold {AccentAlt}][/][bold black on {AccentAlt}] {NerdSpark} Model Comparison Lab [/][bold {AccentAlt}][/][bold {Warning}][/][bold black on {Warning}] {NerdTokens} Tokens [/][bold {Warning}][/][bold {Success}][/][bold black on {Success}] {NerdCost} Cost [/][bold {Success}][/][bold {Accent}]─╯[/]")).Width(BannerInnerWidth),
            Align.Center(new Markup($"[{Muted}]Foundry-style model runs, prompt galleries, latency, tokens, and cost estimates[/]")).Width(BannerInnerWidth),
            Align.Center(new Markup($"[{Muted}]Proudly presented to you by[/] [bold {Accent}]Azure Foundry[/][{Muted}],[/] [bold {Success}]GitHub Copilot[/][{Muted}], and[/] [bold {AccentAlt}]Uwe Baumann[/]")).Width(BannerInnerWidth)
        ]);

        WriteFullWidth(new Panel(title)
            .Border(BoxBorder.Double)
            .BorderColor(AccentColor)
            .Header($" [bold {Accent}][/][bold black on {Accent}] Model World [/][bold {Accent}][/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();
    }

    private static IRenderable[] BuildLogoWordmark()
    {
        string[] lines =
        [
            @"    __  ___          __     __   _       __           __    __",
            @"   /  |/  /___  ____/ /__  / /  | |     / /___  _____/ /___/ /",
            @"  / /|_/ / __ \/ __  / _ \/ /   | | /| / / __ \/ ___/ / __  / ",
            @" / /  / / /_/ / /_/ /  __/ /    | |/ |/ / /_/ / /  / / /_/ /  ",
            @"/_/  /_/\____/\__,_/\___/_/     |__/|__/\____/_/  /_/\__,_/   "
        ];

        return lines
            .Select(line => Align.Center(new Text(line, Style.Parse($"bold {Accent}"))).Width(BannerInnerWidth))
            .ToArray();
    }

    public void RenderModelTable(IReadOnlyList<ModelProfile> models)
    {
        var table = new Table()
            .Title($"[bold {Accent}]{NerdModel} Model Catalog[/]")
            .Border(TableBorder.HeavyHead)
            .BorderColor(AccentColor)
            .Width(LayoutWidth)
            .AddColumn(new TableColumn($"[bold {Accent}]Model[/]").NoWrap())
            .AddColumn(new TableColumn($"[bold {AccentAlt}]Family + Fit[/]"))
            .AddColumn(new TableColumn($"[bold {Success}]Scale[/]").RightAligned())
            .AddColumn(new TableColumn($"[bold {AccentAlt}]{NerdCost} Price / 1M[/]").RightAligned());

        foreach (var model in models)
        {
            table.AddRow(
                $"[bold white]{Markup.Escape(model.DisplayName)}[/]",
                $"[{AccentAlt}]{Markup.Escape(model.Family)}[/]\n[{Muted}]{Markup.Escape(model.RecommendedUseCases)}[/]",
                $"[{Success}]{FormatWholeNumber(model.ContextWindowTokens)} ctx[/]\n[{Warning}]{FormatWholeNumber(model.TypicalLatencyMilliseconds)} ms[/]",
                $"[{Accent}]${FormatCurrencyValue(model.InputCostPerMillionTokensUsd)} in[/]\n[{AccentAlt}]${FormatCurrencyValue(model.OutputCostPerMillionTokensUsd)} out[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void RenderPromptTable(IReadOnlyList<PromptScenario> prompts)
    {
        var table = new Table()
            .Title($"[bold {AccentAlt}]{NerdPrompt} Prompt Gallery[/]")
            .Border(TableBorder.HeavyHead)
            .BorderColor(AccentAltColor)
            .Width(LayoutWidth)
            .AddColumn(new TableColumn($"[bold {AccentAlt}]Domain[/]").NoWrap())
            .AddColumn(new TableColumn($"[bold {Accent}]Title[/]").NoWrap())
            .AddColumn(new TableColumn($"[bold {Success}]Reveals[/]"));

        foreach (var prompt in prompts)
        {
            table.AddRow(
                $"[{AccentAlt}]{Markup.Escape(prompt.Domain)}[/]",
                $"[bold white]{Markup.Escape(prompt.Title)}[/]",
                Markup.Escape(prompt.Reveals));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void RenderResults(IReadOnlyList<SimulationResult> results)
    {
        foreach (var group in results.GroupBy(result => result.Prompt))
        {
            WriteFullWidth(new Rule($"[bold {AccentAlt}]{NerdPrompt} {Markup.Escape(group.Key.Domain)}[/] [grey][/] [bold {Accent}]{Markup.Escape(group.Key.Title)}[/]").RuleStyle(AccentAlt));
            AnsiConsole.WriteLine();
            WriteFullWidth(new Panel($"[{Muted}]{Markup.Escape(group.Key.PromptText)}[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(PanelFillColor)
                .Padding(1, 0)
                .Expand());
            AnsiConsole.WriteLine();

            RenderPromptComparisonTable(group.ToArray());
        }
    }

    public void RenderRunSummary(IReadOnlyList<SimulationResult> results)
    {
        var table = new Table()
            .Title($"[bold {Success}]{NerdRun} Run Summary[/]")
            .Border(TableBorder.Heavy)
            .BorderColor(SuccessColor)
            .Width(LayoutWidth)
            .AddColumn(new TableColumn($"[bold {Accent}]Model[/]").NoWrap())
            .AddColumn(new TableColumn($"[bold {AccentAlt}]Prompt[/]").NoWrap())
            .AddColumn(new TableColumn($"[bold {Warning}]Run[/]").RightAligned())
            .AddColumn(new TableColumn($"[bold {AccentAlt}]{NerdCost} Cost[/]").RightAligned())
            .AddColumn(new TableColumn($"[bold {Success}]Finish[/]").NoWrap());

        foreach (var result in results)
        {
            table.AddRow(
                $"[bold white]{Markup.Escape(result.Model.DisplayName)}[/]",
                $"[{AccentAlt}]{Markup.Escape(result.Prompt.Title)}[/]",
                $"[{Warning}]{FormatWholeNumber(result.Elapsed.TotalMilliseconds)} ms[/]\n[{Success}]{FormatWholeNumber(result.TotalTokens)} tok[/]",
                $"[{Accent}]${FormatCost(result.Cost.TotalCostUsd)}[/]",
                $"[{Success}]{Markup.Escape(result.FinishReason)}[/]");
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
                    .HighlightStyle(Style.Parse($"bold {Accent}"))
                    .MoreChoicesText($"[{Muted}](Move up and down to reveal more models.)[/]")
                    .InstructionsText($"[{Muted}](Press <space> to toggle a model, <enter> to run.)[/]")
                    .Required()
                    .PageSize(8)
                    .AddChoices(choices));

            if (selected.Count <= MaximumComparedModels)
            {
                return selected
                    .Select(displayName => models.First(model => model.DisplayName == displayName))
                    .ToArray();
            }

            AnsiConsole.MarkupLine($"[bold {Warning}]Choose {MaximumComparedModels} or fewer models so the comparison fits in columns.[/]");
        }
    }

    public IReadOnlyList<PromptScenario> SelectPrompts(IReadOnlyList<PromptScenario> prompts)
    {
        var choices = new[] { "All prompts" }.Concat(prompts.Select(prompt => $"{prompt.Domain}: {prompt.Title}")).ToArray();
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a prompt")
                .HighlightStyle(Style.Parse($"bold {AccentAlt}"))
                .AddChoices(choices));

        return selected == "All prompts"
            ? prompts
            : [prompts.First(prompt => selected.EndsWith(prompt.Title, StringComparison.Ordinal))];
    }

    public bool ShouldRunComparison() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .HighlightStyle(Style.Parse($"bold {Accent}"))
                .AddChoices($"{NerdRun} Run a model comparison", $"{NerdExit} Exit Model World")) == $"{NerdRun} Run a model comparison";

    public bool ShouldRunAnotherComparison() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Keep exploring?")
                .HighlightStyle(Style.Parse($"bold {Accent}"))
                .AddChoices($"{NerdRun} Run another comparison", $"{NerdExit} Exit Model World")) == $"{NerdRun} Run another comparison";

    public void RenderGoodbye()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Muted}]Thanks for exploring[/] [bold {Accent}]Model World[/][{Muted}].[/]");
    }

    public async Task ShowProgressAsync(Func<Task> action)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .SpinnerStyle(Style.Parse($"bold {AccentAlt}"))
            .StartAsync($"[{Accent}]{NerdRun} Running static simulation...[/]", async _ =>
            {
                await Task.Delay(250);
                await action();
            });
    }

    private static void RenderPromptComparisonTable(IReadOnlyList<SimulationResult> results)
    {
        var table = new Table()
            .Border(TableBorder.HeavyHead)
            .BorderColor(AccentColor)
            .Width(LayoutWidth)
            .AddColumn(new TableColumn($"[{Muted}]Metric[/]").NoWrap());

        foreach (var result in results)
        {
            table.AddColumn(new TableColumn($"[bold {Accent}]{NerdModel} {Markup.Escape(result.Model.DisplayName)}[/]"));
        }

        table.AddRow(BuildRow($"[{Warning}]󰔟 Elapsed[/]", results, result => $"[{Warning}]{FormatWholeNumber(result.Elapsed.TotalMilliseconds)} ms[/]"));
        table.AddRow(BuildRow($"[{Success}]󰓡 Tokens[/]", results, result => $"[{Accent}]{FormatWholeNumber(result.PromptTokens)}[/] prompt\n[{AccentAlt}]{FormatWholeNumber(result.CompletionTokens)}[/] completion\n[{Success}]{FormatWholeNumber(result.TotalTokens)}[/] total"));
        table.AddRow(BuildRow($"[{AccentAlt}]{NerdCost} Estimated cost[/]", results, result => $"[{Accent}]${FormatCost(result.Cost.TotalCostUsd)}[/]"));
        table.AddRow(BuildRow($"[{Success}]󰄬 Finish[/]", results, result => $"[{Success}]{Markup.Escape(result.FinishReason)}[/]"));
        table.AddRow(BuildRow("Note", results, result => Markup.Escape(result.Note ?? "")));
        table.AddRow(BuildRow($"[bold {Accent}]󰦨 Output[/]", results, result => $"[white]{Markup.Escape(result.Output)}[/]"));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static void WriteFullWidth(IRenderable renderable) =>
        AnsiConsole.Write(Align.Left(renderable).Width(LayoutWidth));

    private static string[] BuildRow(
        string label,
        IReadOnlyList<SimulationResult> results,
        Func<SimulationResult, string> valueFactory) =>
        [label, .. results.Select(valueFactory)];

    private static string FormatWholeNumber(int value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatWholeNumber(double value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatCurrencyValue(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string FormatCost(decimal value) =>
        value.ToString("0.000000", CultureInfo.InvariantCulture);
}