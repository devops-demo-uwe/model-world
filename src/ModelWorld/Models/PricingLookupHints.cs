namespace ModelWorld.Models;

public sealed record PricingLookupHints(
    IReadOnlyList<string> ProductNameContains,
    IReadOnlyList<string> SkuNameContains,
    IReadOnlyList<string> InputMeterNameContains,
    IReadOnlyList<string> OutputMeterNameContains,
    IReadOnlyList<string> DeploymentTypeContains);