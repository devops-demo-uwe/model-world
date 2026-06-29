using ModelWorld.Catalogs;

namespace ModelWorld.Tests;

public sealed class CatalogTests
{
    [Fact]
    public void ModelCatalog_ContainsFiveUniqueModelsWithPricing()
    {
        var models = ModelCatalog.All;

        Assert.Equal(5, models.Count);
        Assert.Equal(5, models.Select(model => model.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(models, model => Assert.True(model.InputCostPerMillionTokensUsd > 0));
        Assert.All(models, model => Assert.True(model.OutputCostPerMillionTokensUsd > 0));
        Assert.All(models, model => Assert.False(string.IsNullOrWhiteSpace(model.RecommendedUseCases)));
    }

    [Fact]
    public void ModelCatalog_DefaultComparisonSetContainsThreeKnownModels()
    {
        var defaultModels = ModelCatalog.GetDefaultComparisonModels();

        Assert.Equal(3, defaultModels.Count);
        Assert.Equal(3, defaultModels.Select(model => model.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(defaultModels, model => Assert.Contains(model, ModelCatalog.All));
    }

    [Fact]
    public void PromptCatalog_ContainsFiveUniquePromptsAcrossExpectedDomains()
    {
        var prompts = PromptCatalog.All;

        Assert.Equal(5, prompts.Count);
        Assert.Equal(5, prompts.Select(prompt => prompt.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var domains = prompts.Select(prompt => prompt.Domain).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Mathematics", domains);
        Assert.Contains("Reasoning", domains);
        Assert.Contains("Coding", domains);
        Assert.Contains("Summarization", domains);
        Assert.Contains("Structured Output", domains);
    }
}