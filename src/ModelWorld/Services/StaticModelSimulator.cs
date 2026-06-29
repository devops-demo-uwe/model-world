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
        "math-check" => 34,
        "reasoning-schedule" => 55,
        "coding-review" => 31,
        "summarization" => 43,
        "structured-output" => 39,
        _ => Math.Max(24, prompt.PromptText.Length / 4)
    };

    private static int EstimateCompletionTokens(ModelProfile model, PromptScenario prompt)
    {
        var baseTokens = prompt.Id switch
        {
            "math-check" => 47,
            "reasoning-schedule" => 68,
            "coding-review" => 54,
            "summarization" => 42,
            "structured-output" => 28,
            _ => 48
        };

        return model.Id switch
        {
            "gpt-55-prototype" => baseTokens + 18,
            "gpt-4o" => baseTokens + 8,
            "gpt-4o-mini" => Math.Max(20, baseTokens - 12),
            "phi-4" => Math.Max(22, baseTokens - 8),
            "llama-31-70b-instruct" => baseTokens + 4,
            _ => baseTokens
        };
    }

    private static int EstimateLatency(ModelProfile model, PromptScenario prompt)
    {
        var promptAdjustment = prompt.Id switch
        {
            "reasoning-schedule" => 340,
            "coding-review" => 180,
            "structured-output" => -120,
            _ => 0
        };

        return Math.Max(250, model.TypicalLatencyMilliseconds + promptAdjustment);
    }

    private static string CreateNote(ModelProfile model, PromptScenario prompt)
    {
        if (model.Id == "gpt-55-prototype")
        {
            return "Prototype-only sample data; values are illustrative and not from a live deployment.";
        }

        if (prompt.Id == "structured-output" && model.Id == "llama-31-70b-instruct")
        {
            return "Simulated format drift: result remains useful but would need validation before automation.";
        }

        return "Static simulation; no Azure request was sent.";
    }

    private static string CreateOutput(ModelProfile model, PromptScenario prompt) => prompt.Id switch
    {
        "math-check" => model.Id switch
        {
            "gpt-4o-mini" => "The sale price is 80% of the original price. Original = 64 / 0.8 = 80, so the jacket was $80.",
            "phi-4" => "Let x be the original price. 0.8x = 64, so x = 64 / 0.8 = 80. The original price was $80.",
            _ => "A 20% discount means the $64 sale price is 80% of the original. Divide by 0.80: $64 / 0.80 = $80. The original price was $80."
        },
        "reasoning-schedule" => model.Id switch
        {
            "gpt-4o-mini" => "The sessions take 25 + 40 + 15 = 80 minutes. 80 minutes before 3:00 PM is 1:40 PM, so setup starts at 1:40 PM.",
            "phi-4" => "Work backward: Q&A starts 2:45 PM, demo starts 2:05 PM, setup starts 1:40 PM. Latest setup start: 1:40 PM.",
            _ => "The sessions must occupy 25 + 40 + 15 = 80 minutes total. Working backward from the 3:00 PM Q&A end time gives 1:40 PM as the latest setup start."
        },
        "coding-review" => model.Id switch
        {
            "gpt-4o-mini" => "Add a guard for count == 0 before dividing, otherwise Average can throw or produce invalid behavior.",
            "phi-4" => "The method should reject a zero count. For example: if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count)); then divide.",
            _ => "The main issue is division by zero. Validate `count` before calculating the average, ideally rejecting `count <= 0` with `ArgumentOutOfRangeException` so callers get a clear failure instead of a runtime divide error."
        },
        "summarization" => model.Id switch
        {
            "gpt-4o-mini" => "The prototype gives PMs a quick way to compare model quality, speed, token use, and estimated cost. Results are static for now, with Foundry integration planned next.",
            "llama-31-70b-instruct" => "Model World now demonstrates side-by-side model comparison using static data. It previews the experience before Azure AI Foundry calls replace the simulated metrics and outputs.",
            _ => "Model World now offers an early side-by-side comparison flow for five AI models, showing outputs, latency, tokens, and estimated cost. The current data is static and illustrative; future versions will replace it with Azure AI Foundry results."
        },
        "structured-output" => model.Id switch
        {
            "llama-31-70b-instruct" => "{\n  \"priority\": \"high\",\n  \"owner\": \"Erin\",\n  \"nextAction\": \"Validate the demo run before Friday.\"\n}\n\nThis blocks the walkthrough, so it should be handled soon.",
            _ => "{\n  \"priority\": \"high\",\n  \"owner\": \"Erin\",\n  \"nextAction\": \"Validate the demo run before Friday\"\n}"
        },
        _ => "This static prototype does not have a canned response for the selected prompt yet."
    };
}