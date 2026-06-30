using ModelWorld.Catalogs;
using ModelWorld.Services;

namespace ModelWorld.Tests;

public sealed class StaticModelSimulatorTests
{
    [Fact]
    public async Task RunAsync_ReturnsDeterministicCostedResults()
    {
        var simulator = new StaticModelSimulator();
        var models = new[] { ModelCatalog.GetById("gpt-54") };
        var prompts = new[] { PromptCatalog.GetById("math-check") };

        var firstRun = await simulator.RunAsync(models, prompts);
        var secondRun = await simulator.RunAsync(models, prompts);

        var firstResult = Assert.Single(firstRun);
        var secondResult = Assert.Single(secondRun);

        Assert.Equal(firstResult, secondResult);
        Assert.True(firstResult.PromptTokens > 0);
        Assert.True(firstResult.CompletionTokens > 0);
        Assert.True(firstResult.Cost.TotalCostUsd > 0);
        Assert.Contains("$337.46", firstResult.Output, StringComparison.Ordinal);
        Assert.Contains("Plan A", firstResult.Output, StringComparison.Ordinal);
        Assert.Contains("$50.15", firstResult.Output, StringComparison.Ordinal);
        Assert.Equal(2, firstResult.Output.Split('\n').Length);
    }

    [Fact]
    public async Task RunAsync_ShowsReasoningTrapContrast()
    {
        var simulator = new StaticModelSimulator();
        var models = new[]
        {
            ModelCatalog.GetById("gpt-54-mini"),
            ModelCatalog.GetById("o4-mini")
        };
        var prompts = new[] { PromptCatalog.GetById("reasoning-schedule") };

        var results = await simulator.RunAsync(models, prompts);

        Assert.Equal(2, results.Count);
        Assert.Contains("10 cents", results.Single(result => result.Model.Id == "gpt-54-mini").Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("5 cents", results.Single(result => result.Model.Id == "o4-mini").Output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_ShowsGeneralKnowledgeEscalationContrast()
    {
        var simulator = new StaticModelSimulator();
        var models = ModelCatalog.GetDefaultComparisonModels();
        var prompts = new[] { PromptCatalog.GetById("general-knowledge-escalation") };

        var results = await simulator.RunAsync(models, prompts);

        Assert.Equal(3, results.Count);
        Assert.All(results, result =>
        {
            Assert.Contains("Mars", result.Output, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Moissan", result.Output, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("canned response", result.Output, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Contains(results, result => result.Output.Contains("epi tou kanikleiou", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(results, result => result.Output.Contains("asekretis", StringComparison.OrdinalIgnoreCase));
    }
}