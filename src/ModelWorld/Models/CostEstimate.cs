namespace ModelWorld.Models;

public sealed record CostEstimate(
    decimal InputCostUsd,
    decimal OutputCostUsd,
    decimal TotalCostUsd);