namespace ModelWorld.Models;

public sealed record SimulationResult(
    ModelProfile Model,
    PromptScenario Prompt,
    string Output,
    int PromptTokens,
    int CompletionTokens,
    TimeSpan Elapsed,
    string FinishReason,
    CostEstimate Cost,
    string? Note)
{
    public int TotalTokens => PromptTokens + CompletionTokens;
}