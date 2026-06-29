using ModelWorld.Catalogs;
using ModelWorld.Services;
using OpenAI.Chat;

namespace ModelWorld.Tests;

#pragma warning disable OPENAI001

public sealed class AzureModelRunnerTests
{
    [Fact]
    public void AzureFoundryOptions_NormalizesOpenAiV1Endpoint()
    {
        var options = new AzureFoundryOptions
        {
            Endpoint = "https://example.openai.azure.com"
        };

        var endpoint = options.GetNormalizedEndpoint();

        Assert.Equal("https://example.openai.azure.com/openai/v1/", endpoint.ToString());
    }

    [Fact]
    public void AzureFoundryOptions_UsesDeploymentOverrideWhenConfigured()
    {
        var model = ModelCatalog.GetById("gpt-54-mini");
        var options = new AzureFoundryOptions
        {
            DeploymentOverrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["gpt-54-mini"] = "classroom-mini"
            }
        };

        var deploymentName = options.GetDeploymentName(model);

        Assert.Equal("classroom-mini", deploymentName);
    }

    [Fact]
    public async Task RunAsync_MapsLiveResponseToSimulationResult()
    {
        var runner = new AzureModelRunner(new AzureFoundryOptions(), new FakeFoundryChatClient(
            new FoundryChatResponse(
                Output: "The original price was $80.",
                PromptTokens: 24,
                CompletionTokens: 12,
                FinishReason: "stop",
                Note: null)));
        var model = ModelCatalog.GetById("gpt-54-mini");
        var prompt = PromptCatalog.GetById("math-check");

        var results = await runner.RunAsync([model], [prompt]);

        var result = Assert.Single(results);
        Assert.Equal("The original price was $80.", result.Output);
        Assert.Equal(24, result.PromptTokens);
        Assert.Equal(12, result.CompletionTokens);
        Assert.Equal(36, result.TotalTokens);
        Assert.Equal("stop", result.FinishReason);
        Assert.True(result.Cost.TotalCostUsd > 0);
        Assert.Equal("Live Azure AI Foundry request.", result.Note);
    }

    [Fact]
    public async Task RunAsync_OmitsTemperatureForO4Mini()
    {
        var runner = new AzureModelRunner(new AzureFoundryOptions(), new FakeFoundryChatClient(
            new FoundryChatResponse(
                Output: "Setup starts at 1:40 PM.",
                PromptTokens: 30,
                CompletionTokens: 20,
                FinishReason: "stop",
                Note: null),
            expectedDeploymentName: "o4-mini",
            expectedMaxOutputTokenCount: 1_000,
            expectedTemperature: null,
            expectedReasoningEffortLevel: ChatReasoningEffortLevel.Low));
        var model = ModelCatalog.GetById("o4-mini");
        var prompt = PromptCatalog.GetById("reasoning-schedule");

        var results = await runner.RunAsync([model], [prompt]);

        var result = Assert.Single(results);
        Assert.Equal("stop", result.FinishReason);
        Assert.Equal(50, result.TotalTokens);
    }

    [Fact]
    public async Task RunAsync_ReturnsErrorResultWhenLiveCallFails()
    {
        var runner = new AzureModelRunner(new AzureFoundryOptions(), new FakeFoundryChatClient(
            new InvalidOperationException("deployment missing")));
        var model = ModelCatalog.GetById("gpt-54-mini");
        var prompt = PromptCatalog.GetById("math-check");

        var results = await runner.RunAsync([model], [prompt]);

        var result = Assert.Single(results);
        Assert.Equal("error", result.FinishReason);
        Assert.Equal(0, result.TotalTokens);
        Assert.Equal(0, result.Cost.TotalCostUsd);
        Assert.Contains("deployment missing", result.Note, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeFoundryChatClient : IFoundryChatClient
    {
        private readonly FoundryChatResponse? response;
        private readonly Exception? exception;
        private readonly string expectedDeploymentName;
        private readonly int expectedMaxOutputTokenCount;
        private readonly float? expectedTemperature;
        private readonly ChatReasoningEffortLevel? expectedReasoningEffortLevel;

        public FakeFoundryChatClient(
            FoundryChatResponse response,
            string expectedDeploymentName = "gpt-5.4-mini",
            int expectedMaxOutputTokenCount = 300,
            float? expectedTemperature = 0.2f,
            ChatReasoningEffortLevel? expectedReasoningEffortLevel = null)
        {
            this.response = response;
            this.expectedDeploymentName = expectedDeploymentName;
            this.expectedMaxOutputTokenCount = expectedMaxOutputTokenCount;
            this.expectedTemperature = expectedTemperature;
            this.expectedReasoningEffortLevel = expectedReasoningEffortLevel;
        }

        public FakeFoundryChatClient(Exception exception)
        {
            this.exception = exception;
            expectedDeploymentName = "gpt-5.4-mini";
            expectedMaxOutputTokenCount = 300;
            expectedTemperature = 0.2f;
            expectedReasoningEffortLevel = null;
        }

        public Task<FoundryChatResponse> CompleteAsync(FoundryChatRequest request, CancellationToken cancellationToken)
        {
            if (exception is not null)
            {
                throw exception;
            }

            Assert.Equal(expectedDeploymentName, request.DeploymentName);
            Assert.Equal(expectedMaxOutputTokenCount, request.MaxOutputTokenCount);
            Assert.Equal(expectedTemperature, request.Temperature);
            Assert.Equal(expectedReasoningEffortLevel, request.ReasoningEffortLevel);
            Assert.True(request.Timeout > TimeSpan.Zero);
            return Task.FromResult(response!);
        }
    }
}

#pragma warning restore OPENAI001
