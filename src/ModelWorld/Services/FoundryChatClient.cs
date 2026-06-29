using Azure.Core;
using Azure.Identity;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel.Primitives;

namespace ModelWorld.Services;

#pragma warning disable OPENAI001

public sealed record FoundryChatRequest(
    string DeploymentName,
    string PromptText,
    int MaxOutputTokenCount,
    float? Temperature,
    ChatReasoningEffortLevel? ReasoningEffortLevel,
    TimeSpan Timeout);

public sealed record FoundryChatResponse(
    string Output,
    int PromptTokens,
    int CompletionTokens,
    string FinishReason,
    string? Note);

public interface IFoundryChatClient
{
    Task<FoundryChatResponse> CompleteAsync(FoundryChatRequest request, CancellationToken cancellationToken);
}

public sealed class OpenAiFoundryChatClient : IFoundryChatClient
{
    private const string SystemPrompt = "You are helping compare model behavior in a short educational benchmark. Answer the user prompt directly and concisely.";

    private readonly Uri endpoint;
    private readonly Dictionary<string, ChatClient> clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly BearerTokenPolicy tokenPolicy;

    public OpenAiFoundryChatClient(Uri endpoint, TokenCredential credential)
    {
        this.endpoint = endpoint;
        tokenPolicy = new BearerTokenPolicy(credential, AzureFoundryOptions.DefaultTokenScope);
    }

    public OpenAiFoundryChatClient(Uri endpoint)
        : this(endpoint, new DefaultAzureCredential())
    {
    }

    public async Task<FoundryChatResponse> CompleteAsync(FoundryChatRequest request, CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(request.Timeout);

        var client = GetClient(request.DeploymentName);
        ChatCompletionOptions options = new()
        {
            MaxOutputTokenCount = request.MaxOutputTokenCount
        };

        if (request.Temperature is not null)
        {
            options.Temperature = request.Temperature.Value;
        }

        if (request.ReasoningEffortLevel is not null)
        {
            options.ReasoningEffortLevel = request.ReasoningEffortLevel.Value;
        }

        ChatCompletion completion = await client.CompleteChatAsync(
            [
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(request.PromptText)
            ],
            options,
            timeoutSource.Token);

        var output = string.Concat(completion.Content.Select(part => part.Text));
        var note = string.IsNullOrWhiteSpace(completion.Refusal)
            ? null
            : $"Model refusal: {completion.Refusal}";

        if (completion.Usage is null)
        {
            throw new InvalidOperationException("Azure response did not include token usage, so Model World cannot report live token cost accurately.");
        }

        return new FoundryChatResponse(
            Output: output,
            PromptTokens: completion.Usage.InputTokenCount,
            CompletionTokens: completion.Usage.OutputTokenCount,
            FinishReason: FormatFinishReason(completion.FinishReason),
            Note: note);
    }

    private ChatClient GetClient(string deploymentName)
    {
        if (!clients.TryGetValue(deploymentName, out var client))
        {
            client = new ChatClient(deploymentName, tokenPolicy, new OpenAIClientOptions
            {
                Endpoint = endpoint
            });
            clients.Add(deploymentName, client);
        }

        return client;
    }

    private static string FormatFinishReason(ChatFinishReason finishReason) => finishReason switch
    {
        ChatFinishReason.Stop => "stop",
        ChatFinishReason.Length => "length",
        ChatFinishReason.ContentFilter => "content_filter",
        ChatFinishReason.ToolCalls => "tool_calls",
        ChatFinishReason.FunctionCall => "function_call",
        _ => finishReason.ToString()
    };
}

#pragma warning restore OPENAI001
