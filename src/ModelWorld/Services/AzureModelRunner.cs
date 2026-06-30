using System.Diagnostics;
using Azure.Identity;
using ModelWorld.Models;
using OpenAI.Chat;
using System.ClientModel;

namespace ModelWorld.Services;

#pragma warning disable OPENAI001

public sealed class AzureModelRunner : IModelRunner
{
    private readonly AzureFoundryOptions options;
    private readonly IFoundryChatClient chatClient;
    private readonly IReadOnlyDictionary<string, ModelPricing> pricingByModelId;

    public AzureModelRunner(AzureFoundryOptions options)
        : this(options, new OpenAiFoundryChatClient(options.GetNormalizedEndpoint()))
    {
    }

    public AzureModelRunner(AzureFoundryOptions options, IReadOnlyDictionary<string, ModelPricing> pricingByModelId)
        : this(options, new OpenAiFoundryChatClient(options.GetNormalizedEndpoint()), pricingByModelId)
    {
    }

    public AzureModelRunner(
        AzureFoundryOptions options,
        IFoundryChatClient chatClient,
        IReadOnlyDictionary<string, ModelPricing>? pricingByModelId = null)
    {
        this.options = options;
        this.chatClient = chatClient;
        this.pricingByModelId = pricingByModelId ?? CreateCatalogPricingMap(ModelWorld.Catalogs.ModelCatalog.Live);
    }

    public async Task<IReadOnlyList<SimulationResult>> RunAsync(
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
                results.Add(await RunSingleAsync(model, prompt, cancellationToken));
            }
        }

        return results;
    }

    private async Task<SimulationResult> RunSingleAsync(
        ModelProfile model,
        PromptScenario prompt,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await chatClient.CompleteAsync(
                new FoundryChatRequest(
                    DeploymentName: options.GetDeploymentName(model),
                    PromptText: prompt.PromptText,
                    MaxOutputTokenCount: GetMaxOutputTokenCount(model),
                    Temperature: GetTemperature(model),
                    ReasoningEffortLevel: GetReasoningEffortLevel(model),
                    Timeout: options.GetRequestTimeout()),
                cancellationToken);

            stopwatch.Stop();
            var cost = EstimateCost(model, response);

            return new SimulationResult(
                Model: model,
                Prompt: prompt,
                Output: response.Output,
                PromptTokens: response.PromptTokens,
                CompletionTokens: response.CompletionTokens,
                Elapsed: stopwatch.Elapsed,
                FinishReason: response.FinishReason,
                Cost: cost,
                Note: response.Note ?? "Live Azure AI Foundry request.");
        }
        catch (Exception exception) when (IsExpectedAzureFailure(exception))
        {
            stopwatch.Stop();
            return CreateFailureResult(model, prompt, stopwatch.Elapsed, exception);
        }
    }

    private static SimulationResult CreateFailureResult(
        ModelProfile model,
        PromptScenario prompt,
        TimeSpan elapsed,
        Exception exception) =>
        new(
            Model: model,
            Prompt: prompt,
            Output: "No model output was returned.",
            PromptTokens: 0,
            CompletionTokens: 0,
            Elapsed: elapsed,
            FinishReason: "error",
            Cost: CostEstimate.Unavailable("No successful model response."),
            Note: CreateFailureNote(exception));

    private CostEstimate EstimateCost(ModelProfile model, FoundryChatResponse response)
    {
        if (!pricingByModelId.TryGetValue(model.Id, out var pricing) || !pricing.IsAvailable)
        {
            return CostEstimate.Unavailable(pricing?.Source);
        }

        return CostCalculator.Estimate(
            response.PromptTokens,
            response.CompletionTokens,
            pricing.InputCostPerMillionTokensUsd,
            pricing.OutputCostPerMillionTokensUsd) with
        {
            Source = pricing.Source
        };
    }

    private static IReadOnlyDictionary<string, ModelPricing> CreateCatalogPricingMap(IReadOnlyList<ModelProfile> models) =>
        models.ToDictionary(
            model => model.Id,
            model => ModelPricing.Available(
                model,
                model.InputCostPerMillionTokensUsd,
                model.OutputCostPerMillionTokensUsd,
                source: "Local catalog pricing",
                region: model.PricingRegion,
                effectiveStartDate: null),
            StringComparer.OrdinalIgnoreCase);

    private static bool IsExpectedAzureFailure(Exception exception) =>
        exception is OperationCanceledException
            or AuthenticationFailedException
            or CredentialUnavailableException
            or ClientResultException
            or InvalidOperationException
            || exception is AggregateException aggregateException && aggregateException.InnerExceptions.Any(IsExpectedAzureFailure);

    private float? GetTemperature(ModelProfile model) =>
        string.Equals(model.Id, "o4-mini", StringComparison.OrdinalIgnoreCase)
            ? null
            : options.GetTemperature();

    private int GetMaxOutputTokenCount(ModelProfile model) =>
        string.Equals(model.Id, "o4-mini", StringComparison.OrdinalIgnoreCase)
            ? Math.Max(options.GetMaxOutputTokenCount(), 1_000)
            : options.GetMaxOutputTokenCount();

    private static ChatReasoningEffortLevel? GetReasoningEffortLevel(ModelProfile model)
    {
        if (string.Equals(model.Id, "o4-mini", StringComparison.OrdinalIgnoreCase))
        {
            return ChatReasoningEffortLevel.Low;
        }

        return null;
    }

    private static string CreateFailureNote(Exception exception) => exception switch
    {
        OperationCanceledException => "Azure request timed out or was cancelled.",
        AggregateException aggregateException when aggregateException.InnerExceptions.Any(innerException => innerException is OperationCanceledException) => "Azure request timed out or was cancelled.",
        CredentialUnavailableException => "No Azure credential was available. Sign in with az login or run in an environment with managed identity.",
        AuthenticationFailedException => "Azure credential authentication failed. Check az login, managed identity, or Entra role assignment.",
        ClientResultException clientException => $"Azure request failed with status {clientException.Status}: {clientException.Message}",
        _ => exception.Message
    };
}

#pragma warning restore OPENAI001
