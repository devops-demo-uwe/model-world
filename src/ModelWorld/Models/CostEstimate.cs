namespace ModelWorld.Models;

public sealed record CostEstimate(
    decimal InputCostUsd,
    decimal OutputCostUsd,
    decimal TotalCostUsd,
    bool IsAvailable = true,
    string? Source = null)
{
    public static CostEstimate Unavailable(string? source = null) =>
        new(
            InputCostUsd: 0,
            OutputCostUsd: 0,
            TotalCostUsd: 0,
            IsAvailable: false,
            Source: source);
}