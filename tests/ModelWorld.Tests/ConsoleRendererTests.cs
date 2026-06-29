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
}