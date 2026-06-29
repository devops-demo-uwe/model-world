using ModelWorld.Catalogs;
using ModelWorld.Services;

namespace ModelWorld.Tests;

public sealed class StaticModelSimulatorTests
{
    [Fact]
    public async Task RunAsync_ReturnsDeterministicCostedResults()
    {
        var simulator = new StaticModelSimulator();
        var models = new[] { ModelCatalog.GetById("gpt-4o") };
        var prompts = new[] { PromptCatalog.GetById("math-check") };

        var firstRun = await simulator.RunAsync(models, prompts);
        var secondRun = await simulator.RunAsync(models, prompts);

        var firstResult = Assert.Single(firstRun);
        var secondResult = Assert.Single(secondRun);

        Assert.Equal(firstResult, secondResult);
        Assert.True(firstResult.PromptTokens > 0);
        Assert.True(firstResult.CompletionTokens > 0);
        Assert.True(firstResult.Cost.TotalCostUsd > 0);
        Assert.Contains("$80", firstResult.Output, StringComparison.Ordinal);
    }
}