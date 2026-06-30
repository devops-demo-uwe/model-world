using ModelWorld.Console;

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
}