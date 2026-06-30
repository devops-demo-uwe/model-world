# Teaching Code Examples

These examples are intentionally abridged for classroom discussion. They are based on the ideas in Model World, but they are not copied from the app source. Use them to explain the shape of the solution before students read the full implementation.

The app does three related things:

1. Connects to an Azure AI Foundry model deployment and retrieves a chat response.
2. Keeps model metadata in small records so the console UI can explain what each model is for.
3. Uses token usage and model pricing metadata to estimate request cost.

## Connect To A Model And Retrieve A Response

Model World uses keyless Microsoft Entra authentication through `DefaultAzureCredential`. In development, that usually means the student has already run `az login`. In Azure, the same code can use managed identity when the identity has permission to invoke the model deployment.

```csharp
using Azure.Core;
using Azure.Identity;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel.Primitives;

// Azure OpenAI / Foundry endpoint using the OpenAI v1 route.
var endpoint = new Uri("https://<resource-name>.openai.azure.com/openai/v1/");

// This deployment name must match a deployed model in Azure AI Foundry.
var deploymentName = "gpt-5.4-mini";

// DefaultAzureCredential tries local developer identity first, then Azure-hosted identities.
TokenCredential credential = new DefaultAzureCredential();

// The OpenAI client needs a bearer token policy for Azure OpenAI / Foundry scope.
var tokenPolicy = new BearerTokenPolicy(
    credential,
    "https://cognitiveservices.azure.com/.default");

var chatClient = new ChatClient(
    deploymentName,
    tokenPolicy,
    new OpenAIClientOptions { Endpoint = endpoint });

var options = new ChatCompletionOptions
{
    MaxOutputTokenCount = 300,
    Temperature = 0.3f
};

ChatCompletion completion = await chatClient.CompleteChatAsync(
    [
        new SystemChatMessage("Answer clearly for a student learning model comparison."),
        new UserChatMessage("Explain why token counts matter when comparing models.")
    ],
    options);

// A completion can contain multiple content parts. For simple text, join the text parts.
var answer = string.Concat(completion.Content.Select(part => part.Text));

Console.WriteLine(answer);
Console.WriteLine($"Finish reason: {completion.FinishReason}");

if (completion.Usage is not null)
{
    Console.WriteLine($"Input tokens: {completion.Usage.InputTokenCount}");
    Console.WriteLine($"Output tokens: {completion.Usage.OutputTokenCount}");
}
```

Teaching notes:

- The endpoint identifies the Azure AI resource.
- The deployment name identifies the model deployment inside that resource.
- The prompt is usually split into a system message and a user message.
- Token usage should come from the model response when the service provides it. Do not make up token counts for cost reporting.

## Set And Get Model Metadata

Model World keeps model facts separate from the code that calls Azure. That makes the app easier to teach: students can inspect the catalog without reading network code.

```csharp
public sealed record DemoModel(
    string Id,
    string DisplayName,
    string DeploymentName,
    string Provider,
    int ContextWindowTokens,
    string Strengths,
    string Limitations,
    decimal InputPricePerMillionTokens,
    decimal OutputPricePerMillionTokens);

var models = new List<DemoModel>
{
    new(
        Id: "mini-general",
        DisplayName: "General Mini Model",
        DeploymentName: "gpt-5.4-mini",
        Provider: "Azure OpenAI in Foundry",
        ContextWindowTokens: 128_000,
        Strengths: "Fast everyday answers and low cost",
        Limitations: "Less reliable on hard multi-step reasoning",
        InputPricePerMillionTokens: 0.60m,
        OutputPricePerMillionTokens: 2.40m),

    new(
        Id: "reasoning-small",
        DisplayName: "Small Reasoning Model",
        DeploymentName: "o4-mini",
        Provider: "Azure OpenAI in Foundry",
        ContextWindowTokens: 128_000,
        Strengths: "Step-by-step reasoning and math checks",
        Limitations: "May be slower than general chat models",
        InputPricePerMillionTokens: 1.10m,
        OutputPricePerMillionTokens: 4.40m)
};

// Get one model by a stable id chosen by the app.
var selectedModel = models.Single(model => model.Id == "mini-general");

// Use metadata to configure a request.
var deploymentName = selectedModel.DeploymentName;

// Use metadata to teach the tradeoff before calling the model.
Console.WriteLine($"{selectedModel.DisplayName}: {selectedModel.Strengths}");
Console.WriteLine($"Watch out for: {selectedModel.Limitations}");
Console.WriteLine($"Context window: {selectedModel.ContextWindowTokens:N0} tokens");
```

Teaching notes:

- `Id` is the app's stable name for selection and lookup.
- `DeploymentName` is the Azure name used when calling the model.
- Keeping pricing fields on the metadata record gives the app a fallback when live pricing lookup is unavailable.
- Catalog metadata should be reviewed over time because model names, availability, and pricing can change.

## Store Pricing Lookup Hints

For live pricing, Model World asks the Azure Retail Prices API for meters that look like the selected model. The API is broad, so the app stores hints that help it find likely input-token and output-token meters.

```csharp
public sealed record PricingHints(
    string[] ProductNameContains,
    string[] SkuNameContains,
    string[] InputMeterContains,
    string[] OutputMeterContains);

public sealed record PricedModel(
    string Id,
    string DisplayName,
    PricingHints PricingHints);

var model = new PricedModel(
    Id: "mini-general",
    DisplayName: "General Mini Model",
    PricingHints: new PricingHints(
        ProductNameContains: ["Azure OpenAI"],
        SkuNameContains: ["mini", "gpt"],
        InputMeterContains: ["input", "inp"],
        OutputMeterContains: ["output", "out"]));

// Later, a pricing provider can use these hints to build a query and score results.
Console.WriteLine($"Search product names containing: {string.Join(", ", model.PricingHints.ProductNameContains)}");
Console.WriteLine($"Search SKU names containing: {string.Join(", ", model.PricingHints.SkuNameContains)}");
```

Teaching notes:

- Pricing lookup hints are metadata, not billing truth.
- The app still needs to check currency, region, meter direction, and unit of measure.
- If the app cannot confidently match both input and output meters, it should report pricing as unavailable instead of guessing.

## Determine Request Pricing From Tokens

Model APIs usually report usage as input tokens and output tokens. Azure model pricing is commonly described as dollars per one million tokens. The estimate is therefore:

```text
input cost  = input tokens  / 1,000,000 * input price per million
output cost = output tokens / 1,000,000 * output price per million
total cost  = input cost + output cost
```

An abridged cost helper can be as small as this:

```csharp
public sealed record RequestCost(decimal InputUsd, decimal OutputUsd, decimal TotalUsd);

public static RequestCost EstimateRequestCost(
    int inputTokens,
    int outputTokens,
    decimal inputUsdPerMillionTokens,
    decimal outputUsdPerMillionTokens)
{
    ArgumentOutOfRangeException.ThrowIfNegative(inputTokens);
    ArgumentOutOfRangeException.ThrowIfNegative(outputTokens);
    ArgumentOutOfRangeException.ThrowIfNegative(inputUsdPerMillionTokens);
    ArgumentOutOfRangeException.ThrowIfNegative(outputUsdPerMillionTokens);

    var inputCost = inputTokens / 1_000_000m * inputUsdPerMillionTokens;
    var outputCost = outputTokens / 1_000_000m * outputUsdPerMillionTokens;

    return new RequestCost(inputCost, outputCost, inputCost + outputCost);
}

var cost = EstimateRequestCost(
    inputTokens: 1_200,
    outputTokens: 500,
    inputUsdPerMillionTokens: 0.60m,
    outputUsdPerMillionTokens: 2.40m);

Console.WriteLine($"Estimated request cost: {cost.TotalUsd:C6}");
```

Teaching notes:

- Use `decimal` for money-style calculations.
- Label the result as an estimate.
- Actual invoices can differ because of region, deployment type, discounts, free grants, taxes, marketplace terms, and price changes.

## Put The Pieces Together

The classroom version of the flow looks like this:

```csharp
// 1. Choose the model metadata from the catalog.
DemoModel selectedModel = models.Single(model => model.Id == "mini-general");

// 2. Use selectedModel.DeploymentName to call Azure AI Foundry.
ChatCompletion completion = await chatClient.CompleteChatAsync(
    [new UserChatMessage("Give one practical example of retrieval augmented generation.")],
    new ChatCompletionOptions { MaxOutputTokenCount = 250 });

// 3. Read text and usage from the response.
var text = string.Concat(completion.Content.Select(part => part.Text));
var usage = completion.Usage ?? throw new InvalidOperationException("No token usage was returned.");

// 4. Combine response usage with catalog pricing.
var estimatedCost = EstimateRequestCost(
    usage.InputTokenCount,
    usage.OutputTokenCount,
    selectedModel.InputPricePerMillionTokens,
    selectedModel.OutputPricePerMillionTokens);

Console.WriteLine(text);
Console.WriteLine($"Estimated cost: {estimatedCost.TotalUsd:C6}");
```

That is the core pattern behind Model World: metadata explains what is being run, the chat client retrieves the model response, and the cost calculator turns reported token usage into an educational estimate.