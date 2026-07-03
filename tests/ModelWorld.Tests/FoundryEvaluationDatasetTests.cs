using System.Text.Json;
using System.Text.Json.Serialization;
using ModelWorld.Catalogs;

namespace ModelWorld.Tests;

public sealed class FoundryEvaluationDatasetTests
{
    [Fact]
    public void FoundryGroundedQaDataset_HasEvaluatorFriendlyFields()
    {
        var datasetPath = Path.Combine(GetRepositoryRoot(), "docs", "data", "foundry-grounded-qa-evaluation.jsonl");
        Assert.True(File.Exists(datasetPath), $"Expected Foundry grounded QA dataset at {datasetPath}.");

        var rows = File.ReadLines(datasetPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<GroundedQaDatasetRow>(line))
            .ToArray();

        Assert.All(rows, Assert.NotNull);

        var datasetRows = rows.Cast<GroundedQaDatasetRow>().ToArray();

        Assert.True(datasetRows.Length >= 8);
        Assert.Equal(
            datasetRows.Length,
            datasetRows.Select(row => row.CaseId).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        foreach (var row in datasetRows)
        {
            Assert.False(string.IsNullOrWhiteSpace(row.CaseId));
            Assert.False(string.IsNullOrWhiteSpace(row.Topic));
            Assert.False(string.IsNullOrWhiteSpace(row.Query));
            Assert.False(string.IsNullOrWhiteSpace(row.Context));
            Assert.False(string.IsNullOrWhiteSpace(row.GroundTruth));
            Assert.DoesNotContain("{{", row.Query, StringComparison.Ordinal);
            Assert.DoesNotContain("{{", row.Context, StringComparison.Ordinal);
            Assert.DoesNotContain("{{", row.GroundTruth, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void FoundryEvaluationDataset_MatchesPromptCatalogAndHasUploadFields()
    {
        var datasetPath = Path.Combine(GetRepositoryRoot(), "docs", "data", "model-world-foundry-evaluation.jsonl");
        Assert.True(File.Exists(datasetPath), $"Expected Foundry evaluation dataset at {datasetPath}.");

        var rows = File.ReadLines(datasetPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<FoundryDatasetRow>(line))
            .ToArray();

        Assert.All(rows, Assert.NotNull);

        var datasetRows = rows.Cast<FoundryDatasetRow>().ToArray();
        var promptsById = PromptCatalog.All.ToDictionary(prompt => prompt.Id, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(promptsById.Count, datasetRows.Length);
        Assert.Equal(
            promptsById.Keys.Order(StringComparer.OrdinalIgnoreCase),
            datasetRows.Select(row => row.ScenarioId).Order(StringComparer.OrdinalIgnoreCase));

        foreach (var row in datasetRows)
        {
            var prompt = promptsById[row.ScenarioId];

            Assert.Equal(prompt.Domain, row.Domain);
            Assert.Equal(prompt.Title, row.Title);
            Assert.Equal(NormalizeLineEndings(prompt.PromptText), NormalizeLineEndings(row.Query));
            Assert.Equal(prompt.ExpectedBehavior, row.GroundTruth);
            Assert.Equal(prompt.Intent, row.Intent);
            Assert.Equal(prompt.Reveals, row.Rubric);
            Assert.True(row.EstimatedPromptTokens > 0);
            Assert.True(row.StaticCompletionTokensMin > 0);
            Assert.True(row.StaticCompletionTokensMax >= row.StaticCompletionTokensMin);
            Assert.Equal(300, row.MaxOutputTokens);
            Assert.Equal(0.2m, row.Temperature);
        }
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ModelWorld.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate the repository root.");
    }

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal);

    private sealed record FoundryDatasetRow(
        [property: JsonPropertyName("scenario_id")] string ScenarioId,
        [property: JsonPropertyName("domain")] string Domain,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("ground_truth")] string GroundTruth,
        [property: JsonPropertyName("intent")] string Intent,
        [property: JsonPropertyName("rubric")] string Rubric,
        [property: JsonPropertyName("estimated_prompt_tokens")] int EstimatedPromptTokens,
        [property: JsonPropertyName("static_completion_tokens_min")] int StaticCompletionTokensMin,
        [property: JsonPropertyName("static_completion_tokens_max")] int StaticCompletionTokensMax,
        [property: JsonPropertyName("max_output_tokens")] int MaxOutputTokens,
        [property: JsonPropertyName("temperature")] decimal Temperature);

    private sealed record GroundedQaDatasetRow(
        [property: JsonPropertyName("case_id")] string CaseId,
        [property: JsonPropertyName("topic")] string Topic,
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("context")] string Context,
        [property: JsonPropertyName("ground_truth")] string GroundTruth);
}