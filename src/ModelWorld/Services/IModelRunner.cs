using ModelWorld.Models;

namespace ModelWorld.Services;

public interface IModelRunner
{
    Task<IReadOnlyList<SimulationResult>> RunAsync(
        IReadOnlyList<ModelProfile> models,
        IReadOnlyList<PromptScenario> prompts,
        CancellationToken cancellationToken = default);
}