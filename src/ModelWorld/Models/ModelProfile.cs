namespace ModelWorld.Models;

public sealed record ModelProfile(
    string Id,
    string DisplayName,
    string Provider,
    string Family,
    string DeploymentName,
    string PricingRegion,
    int ContextWindowTokens,
    string Strengths,
    string Limitations,
    string RecommendedUseCases,
    decimal InputCostPerMillionTokensUsd,
    decimal OutputCostPerMillionTokensUsd,
    int TypicalLatencyMilliseconds,
    string BehaviorNotes,
    PricingLookupHints PricingLookupHints);