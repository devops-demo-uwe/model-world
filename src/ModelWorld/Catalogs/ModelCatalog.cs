using ModelWorld.Models;

namespace ModelWorld.Catalogs;

public static class ModelCatalog
{
    public static IReadOnlyList<string> DefaultComparisonModelIds { get; } =
    [
        "gpt-54-mini",
        "o4-mini",
        "llama-33-70b-instruct"
    ];

    public static IReadOnlyList<string> DefaultLiveComparisonModelIds { get; } =
    [
        "gpt-54-mini",
        "o4-mini",
        "llama-33-70b-instruct"
    ];

    public static IReadOnlyList<ModelProfile> All { get; } =
    [
        new(
            Id: "gpt-54",
            DisplayName: "GPT-5.4",
            Provider: "Azure OpenAI in Foundry",
            Family: "OpenAI GPT",
            DeploymentName: "gpt-5.4",
            ContextWindowTokens: 256_000,
            Strengths: "Strong general quality, careful instruction following, broad synthesis",
            Limitations: "Higher expected cost and latency than compact models",
            RecommendedUseCases: "Quality baseline, complex analysis, coding, structured output",
            InputCostPerMillionTokensUsd: 3.00m,
            OutputCostPerMillionTokensUsd: 12.00m,
            TypicalLatencyMilliseconds: 3_850,
            BehaviorNotes: "Use as the flagship baseline for answer quality and polish."),
        new(
            Id: "gpt-54-mini",
            DisplayName: "GPT-5.4 mini",
            Provider: "Azure OpenAI in Foundry",
            Family: "OpenAI GPT",
            DeploymentName: "gpt-5.4-mini",
            ContextWindowTokens: 128_000,
            Strengths: "Good quality-to-cost tradeoff, fast everyday instruction following",
            Limitations: "Less robust on nuanced judgment and hard multi-step tasks",
            RecommendedUseCases: "Everyday assistant tasks, extraction, drafts, classroom demos",
            InputCostPerMillionTokensUsd: 0.60m,
            OutputCostPerMillionTokensUsd: 2.40m,
            TypicalLatencyMilliseconds: 1_150,
            BehaviorNotes: "Practical baseline for cost-aware production-style workloads."),
        new(
            Id: "o4-mini",
            DisplayName: "o4-mini",
            Provider: "Azure OpenAI in Foundry",
            Family: "OpenAI reasoning",
            DeploymentName: "o4-mini",
            ContextWindowTokens: 128_000,
            Strengths: "Reasoning-focused behavior, math, logic, stepwise planning",
            Limitations: "May be slower or more deliberate than general mini models",
            RecommendedUseCases: "Reasoning contrast, math checks, planning, careful analysis",
            InputCostPerMillionTokensUsd: 1.10m,
            OutputCostPerMillionTokensUsd: 4.40m,
            TypicalLatencyMilliseconds: 2_050,
            BehaviorNotes: "Useful for teaching how reasoning-specialized models differ."),
        new(
            Id: "deepseek-v4-pro",
            DisplayName: "DeepSeek-V4-Pro",
            Provider: "Azure AI Foundry Models",
            Family: "DeepSeek",
            DeploymentName: "DeepSeek-V4-Pro",
            ContextWindowTokens: 128_000,
            Strengths: "Strong non-OpenAI comparison point, coding, reasoning, analysis",
            Limitations: "Provider behavior and response style may differ from OpenAI models",
            RecommendedUseCases: "Cross-provider comparison, coding tasks, reasoning prompts",
            InputCostPerMillionTokensUsd: 0.50m,
            OutputCostPerMillionTokensUsd: 1.50m,
            TypicalLatencyMilliseconds: 2_550,
            BehaviorNotes: "Shows model-family differences beyond size and price."),
        new(
            Id: "llama-33-70b-instruct",
            DisplayName: "Llama 3.3 70B Instruct",
            Provider: "Azure AI Foundry Models",
            Family: "Meta Llama",
            DeploymentName: "Llama-3.3-70B-Instruct",
            ContextWindowTokens: 128_000,
            Strengths: "Open model flexibility, strong summarization, good instruction following",
            Limitations: "May need tighter prompting for exact structured output",
            RecommendedUseCases: "Open-model comparisons, summarization, policy experiments",
            InputCostPerMillionTokensUsd: 0.80m,
            OutputCostPerMillionTokensUsd: 0.80m,
            TypicalLatencyMilliseconds: 2_700,
            BehaviorNotes: "Useful comparison point with a slightly different writing style.")
    ];

    public static IReadOnlyList<ModelProfile> Live { get; } =
        All;

    public static ModelProfile GetById(string id) =>
        All.First(model => string.Equals(model.Id, id, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<ModelProfile> GetDefaultComparisonModels() =>
        DefaultComparisonModelIds.Select(GetById).ToArray();

    public static IReadOnlyList<ModelProfile> GetDefaultLiveComparisonModels() =>
        DefaultLiveComparisonModelIds.Select(GetById).ToArray();
}