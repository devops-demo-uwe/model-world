using ModelWorld.Catalogs;

namespace ModelWorld.Tests;

public sealed class CatalogTests
{
    [Fact]
    public void ModelCatalog_ContainsFiveUniqueModelsWithPricingMetadata()
    {
        var models = ModelCatalog.All;

        Assert.Equal(5, models.Count);
        Assert.Equal(5, models.Select(model => model.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(models, model => Assert.True(model.InputCostPerMillionTokensUsd > 0));
        Assert.All(models, model => Assert.True(model.OutputCostPerMillionTokensUsd > 0));
        Assert.All(models, model => Assert.NotEmpty(model.PricingLookupHints.ProductNameContains));
        Assert.All(models, model => Assert.NotEmpty(model.PricingLookupHints.SkuNameContains));
        Assert.All(models, model => Assert.NotEmpty(model.PricingLookupHints.InputMeterNameContains));
        Assert.All(models, model => Assert.NotEmpty(model.PricingLookupHints.OutputMeterNameContains));
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
    public void ModelCatalog_LiveComparisonSetExcludesPrototypeOnlyModels()
    {
        var liveModels = ModelCatalog.GetDefaultLiveComparisonModels();

        Assert.Equal(3, liveModels.Count);
        Assert.All(liveModels, model => Assert.Contains(model, ModelCatalog.Live));
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

        var reasoningPrompt = PromptCatalog.GetById("reasoning-schedule");
        Assert.Equal("Bat and Ball Trap", reasoningPrompt.Title);
        Assert.Contains("bat and ball", reasoningPrompt.PromptText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("5 cents", reasoningPrompt.ExpectedBehavior, StringComparison.OrdinalIgnoreCase);
    }
}