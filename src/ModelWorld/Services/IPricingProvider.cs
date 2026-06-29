using ModelWorld.Models;

namespace ModelWorld.Services;

public interface IPricingProvider
{
    Task<IReadOnlyDictionary<string, ModelPricing>> GetPricingAsync(
        IReadOnlyList<ModelProfile> models,
        string region,
        CancellationToken cancellationToken = default);
}