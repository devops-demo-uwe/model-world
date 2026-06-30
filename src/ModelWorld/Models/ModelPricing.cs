namespace ModelWorld.Models;

public sealed record ModelPricing(
    string ModelId,
    decimal InputCostPerMillionTokensUsd,
    decimal OutputCostPerMillionTokensUsd,
    string Source,
    string Region,
    DateTimeOffset? EffectiveStartDate,
    bool IsAvailable,
    string? Note)
{
    public static ModelPricing Available(
        ModelProfile model,
        decimal inputCostPerMillionTokensUsd,
        decimal outputCostPerMillionTokensUsd,
        string source,
        string region,
        DateTimeOffset? effectiveStartDate,
        string? note = null) =>
        new(
            model.Id,
            inputCostPerMillionTokensUsd,
            outputCostPerMillionTokensUsd,
            source,
            region,
            effectiveStartDate,
            IsAvailable: true,
            note);

    public static ModelPricing CatalogFallback(ModelProfile model, string source, string region, string note) =>
        Available(
            model,
            model.InputCostPerMillionTokensUsd,
            model.OutputCostPerMillionTokensUsd,
            source,
            region,
            effectiveStartDate: null,
            note);

    public static ModelPricing Unavailable(ModelProfile model, string source, string region, string note) =>
        new(
            model.Id,
            InputCostPerMillionTokensUsd: 0,
            OutputCostPerMillionTokensUsd: 0,
            source,
            region,
            EffectiveStartDate: null,
            IsAvailable: false,
            note);
}