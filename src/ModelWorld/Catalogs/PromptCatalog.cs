using ModelWorld.Models;

namespace ModelWorld.Catalogs;

public static class PromptCatalog
{
    public static IReadOnlyList<PromptScenario> All { get; } =
    [
        new(
            Id: "math-check",
            Domain: "Mathematics",
            Title: "Rental Truck Choice",
            PromptText: """
            You are renting a moving truck for 3 days and will drive 486 miles.

            Plan A costs $79 per day, includes 100 miles per day, and charges $0.59 for each extra mile. Plan A also has a $35 coupon applied before tax.
            Plan B costs $109 per day with unlimited miles, but adds a 9.5% insurance fee on the daily charge.
            Both plans add 8.25% sales tax after discounts, mileage charges, and fees.

            Which plan is cheaper? Return exactly these 2 lines and no extra explanation:
            Plan A total: <amount>
            Cheaper plan: <plan> by <amount>
            """,
            Intent: "Stress everyday multi-step arithmetic, included-mile accounting, selective discounts and fees, tax order, comparison, and strict concise formatting.",
            ExpectedBehavior: "Return only the requested two lines: Plan A total is $337.46, and Plan A is cheaper by $50.15.",
            Reveals: "Whether the model handles included miles across multiple days, extra-mile charges, a coupon applied before tax, an insurance fee that only applies to one plan, tax order, rounding, and concise answer formatting."),
        new(
            Id: "reasoning-schedule",
            Domain: "Reasoning",
            Title: "Bat and Ball Trap",
            PromptText: "A bat and ball cost $1.10. The bat costs $1 more than the ball. How much does the ball cost?",
            Intent: "Reveal whether the model checks the algebra instead of following the common surface-pattern answer.",
            ExpectedBehavior: "Answer 5 cents: if the ball costs $0.05, the bat costs $1.05, and together they cost $1.10.",
            Reveals: "Whether the model slows down to verify the problem structure or returns the tempting but incorrect 10 cents answer."),
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
            PromptText: """
            You are writing a release note brief for a product manager preparing a stakeholder update. Summarize the following change in 45-60 words. Keep it to 2-3 sentences, use concrete product language, and include only the user value, current limitation, and next milestone.

            Context:
            Model World is an educational .NET console app for developers learning AI model evaluation. The prototype can run the same prompt across a curated set of five models and displays each model's response, latency in milliseconds, prompt, completion, and total token usage, finish reason, and estimated cost. The latest demo can use live Azure AI Foundry requests for deployed models, while static simulation remains available for free local demos. The feature is meant to help teams discuss model quality, speed, and cost tradeoffs before choosing a model for a scenario. Pricing is still estimated from catalog metadata and should be treated as illustrative rather than a bill.
            """,
            Intent: "Compare audience-aware synthesis from a realistic product update.",
            ExpectedBehavior: "Produce a short PM-ready brief that explains comparison value, the estimated pricing caveat, and the next Foundry integration milestone.",
            Reveals: "How well the model prioritizes product impact, caveats, and roadmap context for a business audience."),
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