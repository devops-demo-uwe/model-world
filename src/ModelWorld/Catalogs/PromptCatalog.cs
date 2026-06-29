using ModelWorld.Models;

namespace ModelWorld.Catalogs;

public static class PromptCatalog
{
    public static IReadOnlyList<PromptScenario> All { get; } =
    [
        new(
            Id: "math-check",
            Domain: "Mathematics",
            Title: "Discount Reversal",
            PromptText: "A jacket is discounted by 20% and now costs $64. What was the original price? Show the calculation briefly.",
            Intent: "Check arithmetic and explanation quality.",
            ExpectedBehavior: "Recover the original price by dividing 64 by 0.8, yielding $80.",
            Reveals: "Whether the model can avoid the common mistake of adding 20% back to $64."),
        new(
            Id: "reasoning-schedule",
            Domain: "Reasoning",
            Title: "Workshop Schedule",
            PromptText: "Three sessions must run in order: setup before demo, demo before Q&A. Setup takes 25 minutes, demo 40 minutes, Q&A 15 minutes. If Q&A must end by 3:00 PM, when is the latest setup can start?",
            Intent: "Evaluate chained time reasoning.",
            ExpectedBehavior: "Work backward from 3:00 PM for a total of 80 minutes, so setup starts by 1:40 PM.",
            Reveals: "How clearly the model tracks dependencies and units."),
        new(
            Id: "coding-review",
            Domain: "Coding",
            Title: "C# Guard Clause",
            PromptText: "Review this C# method and suggest one improvement: public static decimal Average(decimal total, int count) => total / count;",
            Intent: "Surface practical code review behavior.",
            ExpectedBehavior: "Mention division by zero and recommend validating count before dividing.",
            Reveals: "Whether the model prioritizes real bugs over style nits."),
        new(
            Id: "summarization",
            Domain: "Summarization",
            Title: "Release Note Brief",
            PromptText: "Summarize for a product manager: The prototype now compares five AI models across prompt outputs, latency, tokens, and estimated cost. Data is static today and will later be replaced by Azure AI Foundry calls.",
            Intent: "Compare concise synthesis and audience fit.",
            ExpectedBehavior: "Highlight comparison value, static data caveat, and future Foundry integration.",
            Reveals: "How well the model adapts wording to a business audience."),
        new(
            Id: "structured-output",
            Domain: "Structured Output",
            Title: "JSON Task Extractor",
            PromptText: "Return JSON with fields priority, owner, and nextAction for this note: Erin should validate the demo run before Friday; it blocks the team walkthrough.",
            Intent: "Test instruction following and structured output discipline.",
            ExpectedBehavior: "Return only valid JSON with high priority, owner Erin, and a validation next action.",
            Reveals: "Whether the model obeys strict format requirements without extra prose.")
    ];

    public static PromptScenario GetById(string id) =>
        All.First(prompt => string.Equals(prompt.Id, id, StringComparison.OrdinalIgnoreCase));
}