using ModelWorld.Models;

namespace ModelWorld.Services;

public sealed class StaticModelSimulator : IModelRunner
{
    public Task<IReadOnlyList<SimulationResult>> RunAsync(
        IReadOnlyList<ModelProfile> models,
        IReadOnlyList<PromptScenario> prompts,
        CancellationToken cancellationToken = default)
    {
        List<SimulationResult> results = [];

        foreach (var prompt in prompts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var model in models)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var promptTokens = EstimatePromptTokens(prompt);
                var completionTokens = EstimateCompletionTokens(model, prompt);
                var cost = CostCalculator.Estimate(
                    promptTokens,
                    completionTokens,
                    model.InputCostPerMillionTokensUsd,
                    model.OutputCostPerMillionTokensUsd);

                results.Add(new SimulationResult(
                    Model: model,
                    Prompt: prompt,
                    Output: CreateOutput(model, prompt),
                    PromptTokens: promptTokens,
                    CompletionTokens: completionTokens,
                    Elapsed: TimeSpan.FromMilliseconds(EstimateLatency(model, prompt)),
                    FinishReason: prompt.Id == "structured-output" && model.Id == "llama-31-70b-instruct" ? "length" : "stop",
                    Cost: cost,
                    Note: CreateNote(model, prompt)));
            }
        }

        return Task.FromResult<IReadOnlyList<SimulationResult>>(results);
    }

    private static int EstimatePromptTokens(PromptScenario prompt) => prompt.Id switch
    {
        "math-check" => 126,
        "reasoning-schedule" => 27,
        "coding-review" => 31,
        "summarization" => 165,
        "structured-output" => 39,
        "general-knowledge-escalation" => 90,
        _ => Math.Max(24, prompt.PromptText.Length / 4)
    };

    private static int EstimateCompletionTokens(ModelProfile model, PromptScenario prompt)
    {
        var baseTokens = prompt.Id switch
        {
            "math-check" => 28,
            "reasoning-schedule" => 42,
            "coding-review" => 54,
            "summarization" => 50,
            "structured-output" => 28,
            "general-knowledge-escalation" => 58,
            _ => 48
        };

        return model.Id switch
        {
            "gpt-54" => baseTokens + 14,
            "gpt-54-mini" => Math.Max(20, baseTokens - 12),
            "o4-mini" => baseTokens + 10,
            "deepseek-v4-pro" => baseTokens + 6,
            "llama-33-70b-instruct" => baseTokens + 4,
            _ => baseTokens
        };
    }

    private static int EstimateLatency(ModelProfile model, PromptScenario prompt)
    {
        var promptAdjustment = prompt.Id switch
        {
            "math-check" => 420,
            "reasoning-schedule" => 340,
            "coding-review" => 180,
            "general-knowledge-escalation" => 260,
            "structured-output" => -120,
            _ => 0
        };

        return Math.Max(250, model.TypicalLatencyMilliseconds + promptAdjustment);
    }

    private static string CreateNote(ModelProfile model, PromptScenario prompt)
    {
        if (prompt.Id == "structured-output" && model.Id == "llama-33-70b-instruct")
        {
            return "Simulated format drift: result remains useful but would need validation before automation.";
        }

        return "Static simulation; no Azure request was sent.";
    }

    private static string CreateOutput(ModelProfile model, PromptScenario prompt) => prompt.Id switch
    {
        "math-check" => model.Id switch
        {
            "llama-33-70b-instruct" => "Plan A total: $337.46\nCheaper plan: Plan A by $50.15",
            _ => "Plan A total: **$337.46**\nCheaper plan: **Plan A** by **$50.15**"
        },
        "reasoning-schedule" => model.Id switch
        {
            "gpt-54-mini" => "The ball costs 10 cents.",
            "o4-mini" => "Let the ball cost x. The bat costs x + $1, so 2x + $1 = $1.10. That makes x = $0.05, so the ball costs 5 cents.",
            "llama-33-70b-instruct" => "The ball costs 5 cents. Then the bat costs $1.05, and $1.05 + $0.05 = $1.10.",
            _ => "The ball costs 5 cents: if the ball is $0.05, the bat is $1.05, which is exactly $1.10 total."
        },
        "coding-review" => model.Id switch
        {
            "gpt-54-mini" => "Add a guard for count == 0 before dividing, otherwise Average can throw or produce invalid behavior.",
            "deepseek-v4-pro" => "The method should reject a zero count. For example: if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count)); then divide.",
            _ => "The main issue is division by zero. Validate `count` before calculating the average, ideally rejecting `count <= 0` with `ArgumentOutOfRangeException` so callers get a clear failure instead of a runtime divide error."
        },
        "summarization" => model.Id switch
        {
            "gpt-54-mini" => "Model World helps teams compare model quality, speed, token usage, and estimated cost in one console view. Static demos remain available and pricing is illustrative, while the next milestone is tighter Azure AI Foundry integration for realistic model comparisons.",
            "llama-33-70b-instruct" => "Model World gives stakeholders a compact way to compare five AI models before choosing one for a scenario. Cost remains an estimate and static mode is still available; the next step is improving Foundry coverage and pricing fidelity.",
            _ => "Model World now compares five AI models side by side, helping teams discuss output quality, latency, tokens, and estimated cost. Pricing remains illustrative, and the next milestone is deeper Azure AI Foundry integration for more realistic comparisons."
        },
        "structured-output" => model.Id switch
        {
            "llama-33-70b-instruct" => "{\n  \"priority\": \"high\",\n  \"owner\": \"Erin\",\n  \"nextAction\": \"Validate the demo run before Friday.\"\n}\n\nThis blocks the walkthrough, so it should be handled soon.",
            _ => "{\n  \"priority\": \"high\",\n  \"owner\": \"Erin\",\n  \"nextAction\": \"Validate the demo run before Friday\"\n}"
        },
        "general-knowledge-escalation" => model.Id switch
        {
            "gpt-54-mini" => "1. Mars\n2. Henri Moissan\n3. The title was likely asekretis, an imperial secretary connected with official documents.",
            "o4-mini" => "1. Mars\n2. Henri Moissan\n3. epi tou kanikleiou, also described as the kanikleios, the official associated with the imperial inkstand.",
            "llama-33-70b-instruct" => "1. Mars\n2. Henri Moissan\n3. A plausible title is chartoularios, a Byzantine official involved with records and documents.",
            _ => "1. Mars\n2. Henri Moissan\n3. epi tou kanikleiou, the Byzantine official associated with the imperial inkstand."
        },
        _ => "This static prototype does not have a canned response for the selected prompt yet."
    };
}