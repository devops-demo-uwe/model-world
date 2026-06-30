using ModelWorld.Console;
using ModelWorld.Models;
using ModelWorld.Services;

namespace ModelWorld.Tests;

public sealed class ConsoleRendererTests
{
    [Fact]
    public void FormatResultOutputMarkup_RendersCommonMarkdownBold()
    {
        var markup = ConsoleRenderer.FormatResultOutputMarkup("- **Easy** output = **4,320,000**");

        Assert.Contains("[bold]Easy[/]", markup, StringComparison.Ordinal);
        Assert.Contains("[bold]4,320,000[/]", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("**Easy**", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatResultOutputMarkup_EscapesSpectreMarkupFromModelText()
    {
        var markup = ConsoleRenderer.FormatResultOutputMarkup("Do not treat [red]model text[/] as Spectre markup.");

        Assert.Contains("[[red]]model text[[/]]", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildLogoLineMarkup_UsesYellowShineNearSelectedColumn()
    {
        var markup = ConsoleRenderer.BuildLogoLineMarkup("abcde", shineColumn: 2);

        Assert.Contains("[bold #fde047]bcd[/]", markup, StringComparison.Ordinal);
        Assert.Contains("[bold #fbbf24]a[/]", markup, StringComparison.Ordinal);
        Assert.Contains("[bold #fbbf24]e[/]", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildLogoLineMarkup_UsesStaticAccentWithoutShineColumn()
    {
        var markup = ConsoleRenderer.BuildLogoLineMarkup("abcde", shineColumn: null);

        Assert.Equal("[bold #38bdf8]abcde[/]", markup);
    }

    [Fact]
    public void BuildHelpIntroMarkup_IncludesConfiguredAppVersion()
    {
        var markup = ConsoleRenderer.BuildHelpIntroMarkup();

        Assert.Contains("Version:", markup, StringComparison.Ordinal);
        Assert.Contains("0.0.00", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildModelPromptStatusMarkup_ColorsModelAndPromptSeparately()
    {
        var markup = ConsoleRenderer.BuildModelPromptStatusMarkup("GPT-5", "Reasoning Task");

        Assert.Contains("[bold #f472b6]GPT-5[/]", markup, StringComparison.Ordinal);
        Assert.Contains("[bold #fde047]Reasoning Task[/]", markup, StringComparison.Ordinal);
        Assert.Contains("[#38bdf8] on [/]", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildModelPromptStatusMarkup_EscapesDynamicText()
    {
        var markup = ConsoleRenderer.BuildModelPromptStatusMarkup("[red]model[/]", "[blue]task[/]");

        Assert.Contains("[[red]]model[[/]]", markup, StringComparison.Ordinal);
        Assert.Contains("[[blue]]task[[/]]", markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0.0010, 0.0010, "[#fbbf24](lowest)[/]")]
    [InlineData(0.00121, 0.0010, "[#fbbf24]+21%[/]")]
    public void FormatRunCostComparison_ColorsCostRelativeToLowest(decimal totalCostUsd, decimal lowestCostUsd, string expectedMarkup)
    {
        var markup = ConsoleRenderer.FormatRunCostComparison(
            new CostEstimate(0, 0, totalCostUsd),
            lowestCostUsd);

        Assert.Contains(expectedMarkup, markup, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatRunCostComparison_KeepsUnavailableCostUnchanged()
    {
        var markup = ConsoleRenderer.FormatRunCostComparison(CostEstimate.Unavailable(), lowestCostUsd: 0.001m);

        Assert.Contains("unavailable", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("lowest", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("+", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void SummarizeDistinctValues_ReturnsMixedLabelForMultipleValues()
    {
        var summary = ConsoleRenderer.SummarizeDistinctValues(
            ["Azure Retail Prices API", "Local catalog fallback"],
            "Mixed pricing sources");

        Assert.Equal("Mixed pricing sources", summary);
    }

    [Fact]
    public void SummarizeDistinctValues_ReturnsSingleDistinctValue()
    {
        var summary = ConsoleRenderer.SummarizeDistinctValues(
            ["swedencentral", "SwedenCentral"],
            "mixed regions");

        Assert.Equal("swedencentral", summary);
    }

    [Theory]
    [InlineData(AzureRetailPricesPricingProvider.CatalogFallbackSourceName, "catalog fallback: API pricing unavailable", "*")]
    [InlineData(AzureRetailPricesPricingProvider.SourceName, "API/catalog price mismatch", "!")]
    [InlineData(AzureRetailPricesPricingProvider.SourceName, null, null)]
    public void GetPricingMarker_UsesCompactTableMarkers(string source, string? note, string? expected)
    {
        Assert.Equal(expected, ConsoleRenderer.GetPricingMarker(source, note));
    }

    [Fact]
    public void BuildModelCatalogCaption_PutsScaleExplanationOnNewLineAfterPricingDisclaimer()
    {
        var caption = ConsoleRenderer.BuildModelCatalogCaption("* Catalog fallback used for 1 model(s).");

        Assert.Equal(
            "* Catalog fallback used for 1 model(s).\nScale: ctx = maximum context window in tokens; ms = catalog typical latency estimate.",
            caption);
    }

    [Theory]
    [InlineData("Explain semver in one sentence.", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsValidCustomPromptText_RequiresContent(string promptText, bool expected)
    {
        Assert.Equal(expected, ConsoleRenderer.IsValidCustomPromptText(promptText));
    }

    [Fact]
    public void IsValidCustomPromptText_RejectsPromptsOverLimit()
    {
        var promptText = new string('a', ConsoleRenderer.MaximumCustomPromptCharacters + 1);

        Assert.False(ConsoleRenderer.IsValidCustomPromptText(promptText));
    }

    [Fact]
    public void BuildCustomPromptScenario_TrimsAndLabelsPrompt()
    {
        var scenario = ConsoleRenderer.BuildCustomPromptScenario("  Compare these options.  ");

        Assert.Equal("custom-prompt", scenario.Id);
        Assert.Equal("Custom", scenario.Domain);
        Assert.Equal("User Prompt", scenario.Title);
        Assert.Equal("Compare these options.", scenario.PromptText);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void IsValidModelSelectionCount_RequiresExactlyThreeModels(int selectedModelCount, bool expected)
    {
        Assert.Equal(expected, ConsoleRenderer.IsValidModelSelectionCount(selectedModelCount));
    }
}