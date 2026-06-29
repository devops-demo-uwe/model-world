using ModelWorld.Models;

namespace ModelWorld.Services;

public static class CostCalculator
{
    public static CostEstimate Estimate(
        int promptTokens,
        int completionTokens,
        decimal inputCostPerMillionTokensUsd,
        decimal outputCostPerMillionTokensUsd)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(promptTokens);
        ArgumentOutOfRangeException.ThrowIfNegative(completionTokens);
        ArgumentOutOfRangeException.ThrowIfNegative(inputCostPerMillionTokensUsd);
        ArgumentOutOfRangeException.ThrowIfNegative(outputCostPerMillionTokensUsd);

        var inputCost = promptTokens / 1_000_000m * inputCostPerMillionTokensUsd;
        var outputCost = completionTokens / 1_000_000m * outputCostPerMillionTokensUsd;

        return new CostEstimate(inputCost, outputCost, inputCost + outputCost);
    }
}