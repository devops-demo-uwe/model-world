using ModelWorld.Services;

namespace ModelWorld.Tests;

public sealed class CostCalculatorTests
{
    [Fact]
    public void Estimate_UsesPerMillionTokenPricing()
    {
        var estimate = CostCalculator.Estimate(
            promptTokens: 1_000,
            completionTokens: 2_000,
            inputCostPerMillionTokensUsd: 2.50m,
            outputCostPerMillionTokensUsd: 10.00m);

        Assert.Equal(0.0025m, estimate.InputCostUsd);
        Assert.Equal(0.0200m, estimate.OutputCostUsd);
        Assert.Equal(0.0225m, estimate.TotalCostUsd);
    }

    [Fact]
    public void Estimate_RejectsNegativeTokenCounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CostCalculator.Estimate(-1, 0, 1.00m, 1.00m));
    }
}