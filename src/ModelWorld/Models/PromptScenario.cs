namespace ModelWorld.Models;

public sealed record PromptScenario(
    string Id,
    string Domain,
    string Title,
    string PromptText,
    string Intent,
    string ExpectedBehavior,
    string Reveals);