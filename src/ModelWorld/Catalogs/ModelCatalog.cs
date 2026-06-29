using ModelWorld.Models;

namespace ModelWorld.Catalogs;

public static class ModelCatalog
{
    public static IReadOnlyList<string> DefaultComparisonModelIds { get; } =
    [
        "gpt-55-prototype",
        "gpt-4o",
        "gpt-4o-mini"
    ];

    public static IReadOnlyList<ModelProfile> All { get; } =
    [
        new(
            Id: "gpt-55-prototype",
            DisplayName: "GPT-5.5 (prototype sample)",
            Provider: "Azure OpenAI in Foundry",
            Family: "OpenAI GPT",
            DeploymentName: "gpt-55-prototype-demo",
            ContextWindowTokens: 256_000,
            Strengths: "Deep reasoning, careful instruction following, strong synthesis",
            Limitations: "Prototype-only static estimate; not a verified public deployment",
            RecommendedUseCases: "Complex planning, analysis-heavy coding, executive synthesis",
            InputCostPerMillionTokensUsd: 6.00m,
            OutputCostPerMillionTokensUsd: 18.00m,
            TypicalLatencyMilliseconds: 3_850,
            BehaviorNotes: "Most complete answers, tends to explain tradeoffs and assumptions."),
        new(
            Id: "gpt-4o",
            DisplayName: "GPT-4o",
            Provider: "Azure OpenAI in Foundry",
            Family: "OpenAI GPT",
            DeploymentName: "gpt-4o-demo",
            ContextWindowTokens: 128_000,
            Strengths: "Balanced quality, multimodal readiness, strong general reasoning",
            Limitations: "Can be more verbose than smaller models for simple tasks",
            RecommendedUseCases: "General assistants, coding help, summarization, analysis",
            InputCostPerMillionTokensUsd: 2.50m,
            OutputCostPerMillionTokensUsd: 10.00m,
            TypicalLatencyMilliseconds: 2_250,
            BehaviorNotes: "Usually polished and reliable with good structure."),
        new(
            Id: "gpt-4o-mini",
            DisplayName: "GPT-4o mini",
            Provider: "Azure OpenAI in Foundry",
            Family: "OpenAI GPT",
            DeploymentName: "gpt-4o-mini-demo",
            ContextWindowTokens: 128_000,
            Strengths: "Low cost, fast responses, good everyday instruction following",
            Limitations: "Less robust on multi-step reasoning and nuanced judgment",
            RecommendedUseCases: "Classification, extraction, drafts, lightweight chat",
            InputCostPerMillionTokensUsd: 0.15m,
            OutputCostPerMillionTokensUsd: 0.60m,
            TypicalLatencyMilliseconds: 920,
            BehaviorNotes: "Fast and concise; sometimes skips edge cases."),
        new(
            Id: "phi-4",
            DisplayName: "Phi-4",
            Provider: "Azure AI Foundry Models",
            Family: "Microsoft Phi",
            DeploymentName: "phi-4-demo",
            ContextWindowTokens: 16_000,
            Strengths: "Compact reasoning, STEM tasks, efficient local-style workloads",
            Limitations: "Smaller context and less world knowledge than flagship GPT models",
            RecommendedUseCases: "Math drills, constrained reasoning, cost-aware experiments",
            InputCostPerMillionTokensUsd: 0.25m,
            OutputCostPerMillionTokensUsd: 0.75m,
            TypicalLatencyMilliseconds: 1_150,
            BehaviorNotes: "Direct and efficient; works best with crisp prompts."),
        new(
            Id: "llama-31-70b-instruct",
            DisplayName: "Llama 3.1 70B Instruct",
            Provider: "Azure AI Foundry Models",
            Family: "Meta Llama",
            DeploymentName: "llama-31-70b-instruct-demo",
            ContextWindowTokens: 128_000,
            Strengths: "Open model flexibility, strong summarization, good instruction following",
            Limitations: "May need tighter prompting for exact structured output",
            RecommendedUseCases: "Open-model comparisons, summarization, policy experiments",
            InputCostPerMillionTokensUsd: 1.20m,
            OutputCostPerMillionTokensUsd: 1.20m,
            TypicalLatencyMilliseconds: 2_700,
            BehaviorNotes: "Useful comparison point with a slightly different writing style.")
    ];

    public static ModelProfile GetById(string id) =>
        All.First(model => string.Equals(model.Id, id, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<ModelProfile> GetDefaultComparisonModels() =>
        DefaultComparisonModelIds.Select(GetById).ToArray();
}