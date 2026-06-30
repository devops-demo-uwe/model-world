using System.Globalization;
using System.Text;
using ModelWorld;
using ModelWorld.Models;
using ModelWorld.Services;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ModelWorld.Console;

public enum MainMenuAction
{
    RunComparison,
    ViewHelp,
    ViewEnterpriseCostExample,
    Exit
}

public sealed class ConsoleRenderer
{
    private const int LayoutWidth = 120;
    private const int BannerInnerWidth = LayoutWidth - 4;
    private const int MaximumComparedModels = 3;
    internal const int MaximumCustomPromptCharacters = 2_000;
    private const string Accent = "#38bdf8";
    private const string AccentAlt = "#f472b6";
    private const string Success = "#34d399";
    private const string Warning = "#fbbf24";
    private const string Shine = "#fde047";
    private const string Muted = "#94a3b8";
    private const string NerdSpark = "󰐕";
    private const string NerdModel = "󰚩";
    private const string NerdPrompt = "󰈙";
    private const string NerdRun = "󰐊";
    private const string NerdCost = "󰃭";
    private const string NerdExit = "󰗼";
    private const string NerdHelp = "?";
    private const string NerdAzure = "󰠅";
    private const string NerdChart = "󰄧";
    private const string NerdTimer = "󰔟";
    private const string NerdTokens = "󰓡";

    private static readonly Color AccentColor = new(56, 189, 248);
    private static readonly Color AccentAltColor = new(244, 114, 182);
    private static readonly Color SuccessColor = new(52, 211, 153);
    private static readonly Color WarningColor = new(251, 191, 36);
    private static readonly Color PanelFillColor = new(18, 25, 38);
    private const string SaveCursorPosition = "\u001b[s";
    private const string RestoreCursorPosition = "\u001b[u";
    private const string HideCursor = "\u001b[?25l";
    private const string ShowCursor = "\u001b[?25h";
    private bool hasRenderedIntro;

    public ConsoleRenderer()
    {
        AnsiConsole.Profile.Width = LayoutWidth;
    }

    public void RenderIntro(bool isLiveMode = false)
    {
        if (!System.Console.IsOutputRedirected)
        {
            AnsiConsole.Clear();
        }

        var shouldPlayGlitterEffect = !System.Console.IsOutputRedirected && !hasRenderedIntro;
        hasRenderedIntro = true;

        RenderTitleBanner(playGlitterEffect: shouldPlayGlitterEffect);

        var modePanelText = isLiveMode
            ? $"[bold {Warning}]Live Azure mode[/]: [white]Azure AI Foundry requests will be sent and may incur usage charges. Token usage and latency come from live responses.[/]"
            : $"[bold {Warning}]Static prototype[/]: [white]no Azure calls are made.[/]\n[{Muted}]Model behavior, latency, tokens, and costs are illustrative estimates for exploring the future Foundry flow.[/]";

        WriteFullWidth(new Panel(
                modePanelText)
            .Border(BoxBorder.Double)
            .BorderColor(AccentAltColor)
            .Header($" [bold {AccentAlt}][/][bold black on {AccentAlt}] {(isLiveMode ? "Live" : "Preview")} [/][bold {AccentAlt}][/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();
    }

    private static void RenderTitleBanner(bool playGlitterEffect = false)
    {
        if (playGlitterEffect)
        {
            RenderLogoGlitterEffect();
            return;
        }

        RenderTitleBannerFrame(shineColumn: null);
    }

    private static void RenderLogoGlitterEffect()
    {
        var logoWidth = LogoLines.Max(line => line.Length);
        const int frameCount = 12;
        const int shineWidth = 10;

        System.Console.Write(SaveCursorPosition);
        System.Console.Write(HideCursor);

        try
        {
            for (var frame = 0; frame < frameCount; frame++)
            {
                System.Console.Write(RestoreCursorPosition);
                var shineColumn = -shineWidth + (frame * (logoWidth + shineWidth * 2) / (frameCount - 1));
                RenderTitleBannerFrame(shineColumn);
                Thread.Sleep(45);
            }

            System.Console.Write(RestoreCursorPosition);
            RenderTitleBannerFrame(shineColumn: null);
        }
        finally
        {
            System.Console.Write(ShowCursor);
        }
    }

    private static void RenderTitleBannerFrame(int? shineColumn)
    {
        var title = new Rows(
        [
            Align.Center(new Markup($"[bold {Accent}]╭─[/][bold {AccentAlt}][/][bold black on {AccentAlt}] {NerdAzure} Azure AI Foundry [/][bold {AccentAlt}][/][bold {Success}][/][bold black on {Success}] {NerdChart} Compare [/][bold {Success}][/][bold {Warning}][/][bold black on {Warning}] {NerdTimer} Measure [/][bold {Warning}][/][bold {Accent}]─╮[/]")).Width(BannerInnerWidth),
            .. BuildLogoWordmark(shineColumn),
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

    private static readonly string[] LogoLines =
    [
        @"    __  ___          __     __   _       __           __    __",
        @"   /  |/  /___  ____/ /__  / /  | |     / /___  _____/ /___/ /",
        @"  / /|_/ / __ \/ __  / _ \/ /   | | /| / / __ \/ ___/ / __  / ",
        @" / /  / / /_/ / /_/ /  __/ /    | |/ |/ / /_/ / /  / / /_/ /  ",
        @"/_/  /_/\____/\__,_/\___/_/     |__/|__/\____/_/  /_/\__,_/   "
    ];

    private static IRenderable[] BuildLogoWordmark(int? shineColumn = null)
    {
        return LogoLines
            .Select(line => Align.Center(new Markup(BuildLogoLineMarkup(line, shineColumn))).Width(BannerInnerWidth))
            .ToArray();
    }

    internal static string BuildLogoLineMarkup(string line, int? shineColumn)
    {
        if (shineColumn is null)
        {
            return $"[bold {Accent}]{Markup.Escape(line)}[/]";
        }

        var builder = new StringBuilder();
        var currentColor = string.Empty;

        for (var column = 0; column < line.Length; column++)
        {
            var color = GetLogoColumnColor(column, shineColumn.Value);
            if (!string.Equals(color, currentColor, StringComparison.Ordinal))
            {
                if (currentColor.Length > 0)
                {
                    builder.Append("[/]");
                }

                builder.Append(CultureInfo.InvariantCulture, $"[bold {color}]");
                currentColor = color;
            }

            builder.Append(Markup.Escape(line[column].ToString(CultureInfo.InvariantCulture)));
        }

        if (currentColor.Length > 0)
        {
            builder.Append("[/]");
        }

        return builder.ToString();
    }

    private static string GetLogoColumnColor(int column, int shineColumn)
    {
        var distance = Math.Abs(column - shineColumn);
        return distance switch
        {
            <= 1 => Shine,
            <= 3 => Warning,
            <= 5 => Success,
            _ => Accent
        };
    }

    public void RenderModelTable(
        IReadOnlyList<ModelProfile> models,
        IReadOnlyDictionary<string, ModelPricing>? pricingByModelId = null)
    {
        AnsiConsole.WriteLine();

        var pricingSummary = BuildPricingSummary(models, pricingByModelId);
        var table = new Table()
            .Title($"[bold {Accent}]{NerdModel} Model Catalog[/]\n[{Muted}]{Markup.Escape(pricingSummary.Header)}[/]")
            .Border(TableBorder.HeavyHead)
            .BorderColor(AccentColor)
            .Width(LayoutWidth)
            .AddColumn(new TableColumn($"[bold {Accent}]Model[/]").NoWrap())
            .AddColumn(new TableColumn($"[bold {AccentAlt}]Family + Fit[/]"))
            .AddColumn(new TableColumn($"[bold {Success}]Scale[/]").RightAligned())
            .AddColumn(new TableColumn($"[bold {AccentAlt}]{NerdCost} Price / 1M[/]").RightAligned());

        foreach (var model in models)
        {
            var pricing = GetDisplayPricing(model, pricingByModelId);
            table.AddRow(
                $"[bold white]{Markup.Escape(model.DisplayName)}[/]",
                $"[{AccentAlt}]{Markup.Escape(model.Family)}[/]\n[{Muted}]{Markup.Escape(model.RecommendedUseCases)}[/]",
                $"[{Success}]{FormatWholeNumber(model.ContextWindowTokens)} ctx[/]\n[{Warning}]{FormatWholeNumber(model.TypicalLatencyMilliseconds)} ms[/]",
                FormatPricingCell(pricing));
        }

        table.Caption($"[{Muted}]{Markup.Escape(BuildModelCatalogCaption(pricingSummary))}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void RenderEnterpriseChatCostExample(
        IReadOnlyList<ModelProfile> models,
        EnterpriseChatUsageProfile usageProfile,
        IReadOnlyDictionary<string, ModelPricing>? pricingByModelId = null)
    {
        var assumptions =
            $"[bold white]{Markup.Escape(usageProfile.Name)}[/]\n" +
            $"[{Muted}]{FormatWholeNumber(usageProfile.EmployeeCount)} employees; {FormatPercent(usageProfile.DailyActiveUserRate)} daily active; " +
            $"{FormatWholeNumber(usageProfile.ChatsPerActiveUserPerWorkday)} chats per active user per workday; " +
            $"{FormatWholeNumber(usageProfile.WorkdaysPerMonth)} workdays/month; " +
            $"{FormatWholeNumber(usageProfile.AverageInputTokensPerChat)} input + {FormatWholeNumber(usageProfile.AverageOutputTokensPerChat)} output tokens/chat.[/]";

        WriteFullWidth(new Panel(assumptions)
            .Border(BoxBorder.Rounded)
            .BorderColor(WarningColor)
            .Header($" [bold {Warning}]{NerdCost} Enterprise Cost Example[/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.HeavyHead)
            .BorderColor(WarningColor)
            .Width(LayoutWidth)
            .AddColumn(new TableColumn($"[bold {Accent}]Model[/]").NoWrap())
            .AddColumn(new TableColumn($"[bold {Success}]Monthly usage[/]").RightAligned())
            .AddColumn(new TableColumn($"[bold {AccentAlt}]Estimated model cost[/]").RightAligned())
            .AddColumn(new TableColumn($"[bold {Warning}]Cost / employee[/]").RightAligned());

        foreach (var model in models)
        {
            var pricing = GetDisplayPricing(model, pricingByModelId);
            var estimate = pricing.IsAvailable
                ? CostCalculator.Estimate(
                    usageProfile.MonthlyInputTokens,
                    usageProfile.MonthlyOutputTokens,
                    pricing.InputCostPerMillionTokensUsd,
                    pricing.OutputCostPerMillionTokensUsd)
                : CostEstimate.Unavailable(pricing.Source);
            table.AddRow(
                $"[bold white]{Markup.Escape(model.DisplayName)}[/]",
                $"[{Accent}]{FormatCompactNumber(usageProfile.MonthlyInputTokens)}[/] input\n[{AccentAlt}]{FormatCompactNumber(usageProfile.MonthlyOutputTokens)}[/] output\n[{Success}]{FormatWholeNumber(usageProfile.MonthlyChatCount)}[/] chats",
                FormatMonthlyEstimate(estimate),
                estimate.IsAvailable
                    ? $"[{Warning}]${FormatMonthlyCost(estimate.TotalCostUsd / usageProfile.EmployeeCount)} / mo[/]"
                    : $"[{Muted}]unavailable[/]");
        }

        table.Caption($"[{Muted}]Illustrative estimate only. Excludes hosting, search/retrieval, storage, monitoring, discounts, taxes, and regional price differences.[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public void RenderHelpSection(bool isLiveMode = false)
    {
        var idea = new Markup(BuildHelpIntroMarkup());

        var workflow = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .Expand()
            .AddColumn(new TableColumn(string.Empty).NoWrap())
            .AddColumn(new TableColumn(string.Empty))
            .AddRow($"[bold {Accent}]1[/]", $"[white]Review the model catalog[/] [{Muted}]for family, context window, typical latency, and price per million tokens.[/]")
            .AddRow($"[bold {AccentAlt}]2[/]", $"[white]Choose up to three models[/] [{Muted}]so outputs remain readable in side-by-side columns.[/]")
            .AddRow($"[bold {Success}]3[/]", $"[white]Pick one prompt[/] [{Muted}]to test math, reasoning, coding review, summarization, or structured output behavior.[/]")
            .AddRow($"[bold {Warning}]4[/]", $"[white]Compare the results[/] [{Muted}]for correctness, formatting discipline, speed, token use, finish reason, and estimated cost.[/]");

        var watchouts = new Rows(
        [
            new Markup($"[bold {Warning}]Costs are estimates.[/] [{Muted}]Live prices come from Azure Retail Prices API when matched; actual billing can include discounts, credits, taxes, region changes, marketplace terms, and other services.[/]"),
            new Markup($"[bold {Warning}]Latency varies.[/] [{Muted}]Network conditions, regional load, model warmup, throttling, and prompt length can change run times.[/]"),
            new Markup($"[bold {Warning}]One prompt is not proof.[/] [{Muted}]Repeat interesting cases, vary wording, and use prompts that reflect your users' real work.[/]"),
            new Markup($"[bold {Warning}]Outputs need judgment.[/] [{Muted}]Check facts, math, structured output validity, refusal behavior, and whether the answer follows the requested format.[/]"),
            new Markup(isLiveMode
                ? $"[bold {Warning}]Live mode sends Azure requests.[/] [{Muted}]Each run may incur usage charges and requires configured deployments plus keyless Microsoft Entra access.[/]"
                : $"[bold {Warning}]Static mode is illustrative.[/] [{Muted}]It sends no Azure requests; sample quality, latency, tokens, and costs are deterministic teaching data.[/]")
        ]);

        WriteFullWidth(new Panel(idea)
            .Border(BoxBorder.Double)
            .BorderColor(AccentColor)
            .Header($" [bold {Accent}]{NerdHelp} What Model World Is For[/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();

        WriteFullWidth(new Panel(workflow)
            .Border(BoxBorder.Rounded)
            .BorderColor(SuccessColor)
            .Header($" [bold {Success}]{NerdRun} How To Use It[/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();

        WriteFullWidth(new Panel(watchouts)
            .Border(BoxBorder.Rounded)
            .BorderColor(WarningColor)
            .Header($" [bold {Warning}]What To Watch For[/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();
    }

    internal static string BuildHelpIntroMarkup() =>
        $"[bold {Accent}]Model World is a learning lab, not a leaderboard.[/]\n" +
        $"[bold white]Version:[/] [{Muted}]{Markup.Escape(AppVersion.SemVer)}[/]\n" +
        $"[{Muted}]It runs the same small prompt against a curated model set so you can compare response quality, latency, token usage, finish reason, and estimated cost side by side.[/]\n" +
        $"[{Muted}]Use it to build intuition about model tradeoffs before choosing a model for a real scenario.[/]";

    public void RenderPromptTable(IReadOnlyList<PromptScenario> prompts)
    {
        AnsiConsole.WriteLine();

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

    public IReadOnlyList<ModelProfile> SelectModels(IReadOnlyList<ModelProfile> models)
    {
        var choices = models.Select(model => model.DisplayName).ToArray();

        while (true)
        {
            var selected = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title($"Choose exactly {MaximumComparedModels} models to compare")
                    .HighlightStyle(Style.Parse($"bold {Accent}"))
                    .MoreChoicesText($"[{Muted}](Move up and down to reveal more models.)[/]")
                    .InstructionsText($"[{Muted}](Press <space> to toggle models, <enter> to run with exactly {MaximumComparedModels}.)[/]")
                    .Required()
                    .PageSize(8)
                    .AddChoices(choices));

            if (IsValidModelSelectionCount(selected.Count))
            {
                return selected
                    .Select(displayName => models.First(model => model.DisplayName == displayName))
                    .ToArray();
            }

            AnsiConsole.MarkupLine($"[bold {Warning}]Choose exactly {MaximumComparedModels} models so the comparison shows a complete side-by-side set.[/]");
        }
    }

    internal static bool IsValidModelSelectionCount(int selectedModelCount) =>
        selectedModelCount == MaximumComparedModels;

    public IReadOnlyList<PromptScenario> SelectPrompts(IReadOnlyList<PromptScenario> prompts)
    {
        var customPromptChoice = $"{NerdPrompt} Custom: enter your own prompt";
        var choices = prompts
            .Select(prompt => $"{prompt.Domain}: {prompt.Title}")
            .Append(customPromptChoice)
            .ToArray();
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a prompt")
                .HighlightStyle(Style.Parse($"bold {AccentAlt}"))
                .AddChoices(choices));

        if (selected == customPromptChoice)
        {
            var promptText = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter a custom prompt [[max {MaximumCustomPromptCharacters.ToString(CultureInfo.InvariantCulture)} characters]]")
                    .PromptStyle(Style.Parse($"bold {AccentAlt}"))
                    .Validate(input => IsValidCustomPromptText(input)
                        ? ValidationResult.Success()
                        : ValidationResult.Error($"Prompt must be 1-{MaximumCustomPromptCharacters.ToString(CultureInfo.InvariantCulture)} characters.")));

            return [BuildCustomPromptScenario(promptText)];
        }

        return [prompts.First(prompt => selected.EndsWith(prompt.Title, StringComparison.Ordinal))];
    }

    internal static bool IsValidCustomPromptText(string promptText) =>
        !string.IsNullOrWhiteSpace(promptText) && promptText.Length <= MaximumCustomPromptCharacters;

    internal static PromptScenario BuildCustomPromptScenario(string promptText) =>
        new(
            Id: "custom-prompt",
            Domain: "Custom",
            Title: "User Prompt",
            PromptText: promptText.Trim(),
            Intent: "User-provided prompt for an ad hoc comparison.",
            ExpectedBehavior: "Evaluate whether each model follows the user's custom instruction.",
            Reveals: "How selected models respond to the user's own scenario.");

    public MainMenuAction SelectMainMenuAction()
    {
        var runComparison = $"{NerdRun} Run a model comparison";
        var viewHelp = $"{NerdHelp} Help: how to run better comparisons";
        var viewEnterpriseCostExample = $"{NerdCost} View enterprise cost example";
        var exit = $"{NerdExit} Exit Model World";

        RenderMainMenuDeck();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold {Accent}]Choose a command[/]")
                .HighlightStyle(Style.Parse($"bold {Accent}"))
                .AddChoices(runComparison, viewHelp, viewEnterpriseCostExample, exit));

        if (selected == runComparison)
        {
            return MainMenuAction.RunComparison;
        }

        if (selected == viewHelp)
        {
            return MainMenuAction.ViewHelp;
        }

        return selected == viewEnterpriseCostExample
            ? MainMenuAction.ViewEnterpriseCostExample
            : MainMenuAction.Exit;
    }

    private static void RenderMainMenuDeck()
    {
        var commands = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .Expand()
            .AddColumn(new TableColumn(string.Empty).NoWrap())
            .AddColumn(new TableColumn(string.Empty))
            .AddColumn(new TableColumn(string.Empty))
            .AddRow(
                $"[bold {Accent}]{NerdRun} Compare Models[/]",
                $"[{Muted}]Run selected prompts side-by-side[/]",
                $"[{Success}]latency · tokens · cost[/]")
            .AddRow(
                $"[bold {Success}]{NerdHelp} Help[/]",
                $"[{Muted}]Learn the workflow and benchmark caveats[/]",
                $"[{Success}]quality · repeatability · judgment[/]")
            .AddRow(
                $"[bold {Warning}]{NerdCost} Enterprise Cost[/]",
                $"[{Muted}]Estimate monthly workplace usage[/]",
                $"[{Warning}]usage profile · model pricing[/]")
            .AddRow(
                $"[bold {AccentAlt}]{NerdExit} Exit[/]",
                $"[{Muted}]Leave the comparison lab[/]",
                $"[{AccentAlt}]return to shell[/]");

        WriteFullWidth(new Panel(commands)
            .Border(BoxBorder.Double)
            .BorderColor(AccentColor)
            .Header($" [bold {Accent}][/][bold black on {Accent}] Command Deck [/][bold {Accent}][/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();
    }

    public bool ShouldRunAnotherComparison() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Keep exploring?")
                .HighlightStyle(Style.Parse($"bold {Accent}"))
                .AddChoices($"{NerdRun} Run another comparison", $"{NerdExit} Exit Model World")) == $"{NerdRun} Run another comparison";

    public void WaitForMainMenuReturn() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Enterprise cost example")
                .HighlightStyle(Style.Parse($"bold {Accent}"))
                .AddChoices($"{NerdRun} Return to main menu"));

    public void WaitForHelpReturn() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Help")
                .HighlightStyle(Style.Parse($"bold {Accent}"))
                .AddChoices($"{NerdRun} Return to main menu"));

    public void RenderGoodbye()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Muted}]Thanks for exploring[/] [bold {Accent}]Model World[/][{Muted}].[/]");
    }

    public void RenderConfigurationError(string message)
    {
        WriteFullWidth(new Panel($"[bold {Warning}]Live Azure mode is not configured.[/]\n[white]{Markup.Escape(message)}[/]")
            .Border(BoxBorder.Double)
            .BorderColor(WarningColor)
            .Header($" [bold {Warning}]Configuration[/] ")
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();
    }

    public async Task ShowProgressAsync(Func<Task> action, string statusMessage = "Running static simulation...")
    {
        await ShowProgressAsync(async _ => await action(), statusMessage);
    }

    public async Task ShowProgressAsync(Func<Action<string>, Task> action, string statusMessage = "Running static simulation...")
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .SpinnerStyle(Style.Parse($"bold {AccentAlt}"))
            .StartAsync(BuildProgressStatusMarkup(statusMessage), async context =>
            {
                await Task.Delay(250);
                await action(message => context.Status(BuildProgressStatusMarkup(message)));
            });
    }

    public async Task ShowProgressMarkupAsync(Func<Action<string>, Task> action, string statusMarkup)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .SpinnerStyle(Style.Parse($"bold {AccentAlt}"))
            .StartAsync(statusMarkup, async context =>
            {
                await Task.Delay(250);
                await action(message => context.Status(message));
            });
    }

    internal static string BuildProgressStatusMarkup(string statusMessage) =>
        $"[{Accent}]{NerdRun} {Markup.Escape(statusMessage)}[/]";

    internal static string BuildModelPromptStatusMarkup(string modelDisplayName, string promptTitle) =>
        $"[{Accent}]{NerdRun} Running [/][bold {AccentAlt}]{Markup.Escape(modelDisplayName)}[/]" +
        $"[{Accent}] on [/][bold {Shine}]{Markup.Escape(promptTitle)}[/][{Accent}]...[/]";

    private static void RenderPromptComparisonTable(IReadOnlyList<SimulationResult> results)
    {
        var availableCosts = results
            .Where(result => result.Cost.IsAvailable)
            .Select(result => result.Cost.TotalCostUsd)
            .ToArray();
        var lowestCostUsd = availableCosts.Length > 0
            ? availableCosts.Min()
            : (decimal?)null;

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
    table.AddRow(BuildRow($"[{AccentAlt}]{NerdCost} Estimated cost[/]", results, result => FormatRunCostComparison(result.Cost, lowestCostUsd)));
        table.AddRow(BuildRow($"[{Success}]󰄬 Finish[/]", results, result => $"[{Success}]{Markup.Escape(result.FinishReason)}[/]"));
        table.AddRow(BuildRow($"[{Muted}]Note[/]", results, result => $"[{Muted}]{Markup.Escape(result.Note ?? "")}[/]"));
        table.AddRow(BuildRenderableRow($"[bold {Accent}]󰦨 Output[/]", results, result => FormatResultOutput(result.Output)));

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

    private static IRenderable[] BuildRenderableRow(
        string label,
        IReadOnlyList<SimulationResult> results,
        Func<SimulationResult, IRenderable> valueFactory) =>
        [new Markup(label), .. results.Select(valueFactory)];

    private static IRenderable FormatResultOutput(string output)
    {
        var formattedOutput = FormatResultOutputMarkup(output);
        var markup = new Markup(formattedOutput);

        try
        {
            _ = markup.GetSegments(AnsiConsole.Console).FirstOrDefault();
            return markup;
        }
        catch (Exception)
        {
            return new Markup($"[white]{Markup.Escape(output)}[/]");
        }
    }

    internal static string FormatResultOutputMarkup(string output)
    {
        var builder = new StringBuilder(output.Length + 32);
        var lines = output.ReplaceLineEndings("\n").Split('\n');

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            if (lineIndex > 0)
            {
                builder.AppendLine();
            }

            builder.Append(FormatMarkdownLine(lines[lineIndex]));
        }

        return builder.ToString();
    }

    private static string FormatMarkdownLine(string line)
    {
        var trimmed = line.TrimStart();
        var leadingWhitespace = line[..^trimmed.Length];

        if (trimmed.StartsWith("### ", StringComparison.Ordinal))
        {
            return Markup.Escape(leadingWhitespace) + $"[bold {Accent}]{FormatInlineMarkdown(trimmed[4..])}[/]";
        }

        if (trimmed.StartsWith("## ", StringComparison.Ordinal))
        {
            return Markup.Escape(leadingWhitespace) + $"[bold {AccentAlt}]{FormatInlineMarkdown(trimmed[3..])}[/]";
        }

        if (trimmed.StartsWith("# ", StringComparison.Ordinal))
        {
            return Markup.Escape(leadingWhitespace) + $"[bold {Success}]{FormatInlineMarkdown(trimmed[2..])}[/]";
        }

        return FormatInlineMarkdown(line);
    }

    private static string FormatInlineMarkdown(string text)
    {
        var builder = new StringBuilder(text.Length + 16);

        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '`')
            {
                var endIndex = text.IndexOf('`', index + 1);
                if (endIndex > index)
                {
                    builder.Append($"[grey on #1f2937] {Markup.Escape(text[(index + 1)..endIndex])} [/]");
                    index = endIndex;
                    continue;
                }
            }

            if (index + 1 < text.Length && text[index] == '*' && text[index + 1] == '*')
            {
                var endIndex = text.IndexOf("**", index + 2, StringComparison.Ordinal);
                if (endIndex > index)
                {
                    builder.Append($"[bold]{Markup.Escape(text[(index + 2)..endIndex])}[/]");
                    index = endIndex + 1;
                    continue;
                }
            }

            if (index + 1 < text.Length && text[index] == '_' && text[index + 1] == '_')
            {
                var endIndex = text.IndexOf("__", index + 2, StringComparison.Ordinal);
                if (endIndex > index)
                {
                    builder.Append($"[bold]{Markup.Escape(text[(index + 2)..endIndex])}[/]");
                    index = endIndex + 1;
                    continue;
                }
            }

            builder.Append(Markup.Escape(text[index].ToString()));
        }

        return builder.ToString();
    }

    private static DisplayPricing GetDisplayPricing(
        ModelProfile model,
        IReadOnlyDictionary<string, ModelPricing>? pricingByModelId)
    {
        if (pricingByModelId is null)
        {
            return new DisplayPricing(
                model.InputCostPerMillionTokensUsd,
                model.OutputCostPerMillionTokensUsd,
                IsAvailable: true,
                Source: "Local catalog pricing",
                Region: "static",
                EffectiveStartDate: null);
        }

        if (pricingByModelId.TryGetValue(model.Id, out var pricing) && pricing.IsAvailable)
        {
            return new DisplayPricing(
                pricing.InputCostPerMillionTokensUsd,
                pricing.OutputCostPerMillionTokensUsd,
                IsAvailable: true,
                pricing.Source,
                pricing.Region,
                pricing.EffectiveStartDate);
        }

        return new DisplayPricing(
            InputCostPerMillionTokensUsd: 0,
            OutputCostPerMillionTokensUsd: 0,
            IsAvailable: false,
            Source: pricingByModelId.TryGetValue(model.Id, out var unavailablePricing)
                ? unavailablePricing.Source
                : AzureRetailPricesPricingProvider.SourceName,
            Region: pricingByModelId.TryGetValue(model.Id, out var regionPricing)
                ? regionPricing.Region
                : "unknown",
            EffectiveStartDate: null);
    }

    private static string FormatPricingCell(DisplayPricing pricing)
    {
        if (!pricing.IsAvailable)
        {
            return $"[{Muted}]pricing unavailable[/]";
        }

        return $"[{Accent}]${FormatCurrencyValue(pricing.InputCostPerMillionTokensUsd)} in[/]\n" +
            $"[{AccentAlt}]${FormatCurrencyValue(pricing.OutputCostPerMillionTokensUsd)} out[/]";
    }

    private static string BuildModelCatalogCaption(PricingSummary pricingSummary)
    {
        const string scaleExplanation = "Scale: ctx = maximum context window in tokens; ms = catalog typical latency estimate.";

        return string.IsNullOrWhiteSpace(pricingSummary.Caption)
            ? scaleExplanation
            : $"{pricingSummary.Caption} {scaleExplanation}";
    }

    private static PricingSummary BuildPricingSummary(
        IReadOnlyList<ModelProfile> models,
        IReadOnlyDictionary<string, ModelPricing>? pricingByModelId)
    {
        var displayPricing = models
            .Select(model => GetDisplayPricing(model, pricingByModelId))
            .ToArray();
        var source = displayPricing
            .Select(pricing => pricing.Source)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SingleOrDefault() ?? "Mixed pricing sources";
        var region = displayPricing
            .Select(pricing => pricing.Region)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .SingleOrDefault() ?? "mixed regions";
        var availablePricing = displayPricing
            .Where(pricing => pricing.IsAvailable)
            .ToArray();

        var header = $"Pricing: {source}; region: {region}; USD per 1M tokens";
        if (availablePricing.Length == 0)
        {
            return new PricingSummary(header, "No confident pricing meter matches were found; costs are shown as unavailable.");
        }

        var effectiveDates = availablePricing
            .Select(pricing => pricing.EffectiveStartDate)
            .OfType<DateTimeOffset>()
            .Select(date => date.Date)
            .Distinct()
            .Order()
            .ToArray();
        var unavailableCount = displayPricing.Count(pricing => !pricing.IsAvailable);
        var captionParts = new List<string>();

        if (effectiveDates.Length == 1)
        {
            header += $"; meter date: {effectiveDates[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
        }
        else if (effectiveDates.Length > 1)
        {
            header += $"; meter dates: {effectiveDates[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} to {effectiveDates[^1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
        }

        if (unavailableCount > 0)
        {
            captionParts.Add($"Pricing unavailable for {unavailableCount} model(s).");
        }

        return new PricingSummary(header, string.Join(' ', captionParts));
    }

    internal static string FormatRunCostComparison(CostEstimate estimate, decimal? lowestCostUsd)
    {
        if (!estimate.IsAvailable)
        {
            return $"[{Muted}]unavailable[/]";
        }

        var formattedCost = $"${FormatCost(estimate.TotalCostUsd)}";
        if (lowestCostUsd is null)
        {
            return $"[{Accent}]{formattedCost}[/]";
        }

        if (estimate.TotalCostUsd == lowestCostUsd.Value)
        {
            return $"[{Accent}]{formattedCost}[/]\n[{Success}](lowest)[/]";
        }

        if (lowestCostUsd.Value <= 0)
        {
            return $"[{Accent}]{formattedCost}[/]";
        }

        var percentDifference = (estimate.TotalCostUsd - lowestCostUsd.Value) / lowestCostUsd.Value * 100m;
        return $"[{Accent}]{formattedCost}[/]\n[{Warning}]+{percentDifference.ToString("0", CultureInfo.InvariantCulture)}%[/]";
    }

    private static string FormatMonthlyEstimate(CostEstimate estimate) =>
        estimate.IsAvailable
            ? $"[{AccentAlt}]${FormatMonthlyCost(estimate.TotalCostUsd)} / mo[/]"
            : $"[{Muted}]unavailable[/]";

    private static string FormatWholeNumber(int value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatWholeNumber(double value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatCurrencyValue(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string FormatCost(decimal value) =>
        value.ToString("0.000000", CultureInfo.InvariantCulture);

    private static string FormatMonthlyCost(decimal value) =>
        value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FormatPercent(decimal value) =>
        $"{value * 100m:0}%";

    private static string FormatCompactNumber(decimal value)
    {
        if (value >= 1_000_000_000m)
        {
            return $"{(value / 1_000_000_000m).ToString("0.0", CultureInfo.InvariantCulture)}B";
        }

        if (value >= 1_000_000m)
        {
            return $"{(value / 1_000_000m).ToString("0.0", CultureInfo.InvariantCulture)}M";
        }

        if (value >= 1_000m)
        {
            return $"{(value / 1_000m).ToString("0.0", CultureInfo.InvariantCulture)}K";
        }

        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    private sealed record DisplayPricing(
        decimal InputCostPerMillionTokensUsd,
        decimal OutputCostPerMillionTokensUsd,
        bool IsAvailable,
        string Source,
        string Region,
        DateTimeOffset? EffectiveStartDate);

    private sealed record PricingSummary(
        string Header,
        string Caption);
}