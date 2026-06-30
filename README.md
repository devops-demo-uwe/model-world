# Model World

Model World is an educational .NET console learning lab for comparing AI model behavior across representative prompts.

The app supports two modes:

- Live Azure mode, the default, calls Azure AI Foundry/OpenAI v1 chat completions through the OpenAI .NET client with Microsoft Entra ID keyless authentication.
- Static mode, enabled with `--static`, uses deterministic sample outputs, local catalog pricing, and sends no Azure requests.

## Prototype Contents

- 5 configured model profiles for the deployed classroom comparison set: `gpt-5.4`, `gpt-5.4-mini`, `o4-mini`, `DeepSeek-V4-Pro`, and `Llama-3.3-70B-Instruct`.
- 6 prompt scenarios covering mathematics, reasoning, coding, summarization, structured output, and general knowledge escalation.
- Scripted demo runs that execute one prompt against the default three-model set: `gpt-5.4-mini`, `o4-mini`, and `Llama-3.3-70B-Instruct`.
- Interactive comparison runs that require exactly 3 selected models so generated outputs fit in column-based result tables.
- A custom prompt option for ad hoc comparisons, capped at 2,000 characters.
- Side-by-side result summaries with latency, tokens, estimated cost, finish reason, and generated output.
- Live pricing lookup through Azure Retail Prices API with local catalog pricing as the static-mode fallback.
- A structured instructor demo guide for running benchmarks in an order that highlights model-evaluation tradeoffs.
- A medium-enterprise chat app cost example that translates current or illustrative pricing into monthly spend.
- An interactive command deck with model comparison, help, enterprise cost, and exit commands.
- A Spectre.Console interface for readable tables and result panels.
- Unit tests for deterministic logic, catalog shape, Azure runner behavior, pricing lookup, and console formatting helpers.
- Keyless-only Azure integration through `DefaultAzureCredential`.

## Requirements

- .NET 10 SDK
- Azure CLI or another `DefaultAzureCredential` source, an Azure AI Foundry/OpenAI resource, deployed chat models, and an Entra role assignment that can invoke those deployments.

## Configure Azure Access

Live mode is the normal operating mode and may incur Azure usage charges.

```powershell
az login
dotnet user-secrets set "ModelWorld:Azure:Endpoint" "https://<resource-name>.openai.azure.com/openai/v1/" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:Region" "eastus" --project src\ModelWorld\ModelWorld.csproj
```

The endpoint can be either the base resource URL or the full `/openai/v1/` route. The app normalizes it before creating the chat client.

The default catalog expects these deployment names exactly:

| Model | Expected deployment name |
| --- | --- |
| GPT-5.4 | `gpt-5.4` |
| GPT-5.4 mini | `gpt-5.4-mini` |
| o4-mini | `o4-mini` |
| DeepSeek-V4-Pro | `DeepSeek-V4-Pro` |
| Llama 3.3 70B Instruct | `Llama-3.3-70B-Instruct` |

If your Azure deployment names differ, configure deployment overrides by model id:

```powershell
dotnet user-secrets set "ModelWorld:Azure:DeploymentOverrides:gpt-54-mini" "classroom-mini" --project src\ModelWorld\ModelWorld.csproj
```

Other supported settings are `ModelWorld:Azure:MaxOutputTokenCount`, `ModelWorld:Azure:Temperature`, `ModelWorld:Azure:RequestTimeoutSeconds`, and `ModelWorld:Azure:PricingEndpoint`.

## Run Connected Mode

```powershell
dotnet restore ModelWorld.slnx
dotnet build ModelWorld.slnx
dotnet test ModelWorld.slnx
dotnet run --project src\ModelWorld -- --demo
```

The demo sends three live Azure requests: the `math-check` prompt against the default comparison set of `gpt-5.4-mini`, `o4-mini`, and `Llama-3.3-70B-Instruct`. Choose a different scenario with `--prompt <id>`:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt structured-output
```

Current prompt ids are `math-check`, `structured-output`, `summarization`, `reasoning-schedule`, `coding-review`, and `general-knowledge-escalation`.

The interactive console includes an illustrative monthly cost scenario for a fictitious medium corporate chat app:

| Assumption | Value |
| --- | --- |
| Employees | 1,500 |
| Daily active usage | 35% |
| Chats per active user per workday | 12 |
| Workdays per month | 22 |
| Average input tokens per chat | 1,200 |
| Average output tokens per chat | 500 |

This produces 138,600 chats per month, about 166.3M input tokens, and about 69.3M output tokens. In live mode, the app looks up model pricing once at startup through Azure Retail Prices API for the configured Azure region. If a model's input and output meters cannot be matched confidently, the app shows pricing as unavailable rather than guessing. Treat this as a teaching estimate, not an Azure billing forecast.

For the interactive selection flow and enterprise cost example, run:

```powershell
dotnet run --project src\ModelWorld
```

To read the help section without starting a live run or validating Azure configuration:

```powershell
dotnet run --project src\ModelWorld -- --help
```

## Run Static Mode

Static mode is kept as an offline fallback for demos, CI-style screenshots, or development without Azure access.

```powershell
dotnet restore ModelWorld.slnx
dotnet build ModelWorld.slnx
dotnet test ModelWorld.slnx
dotnet run --project src\ModelWorld -- --static --demo
```

The static demo also defaults to `math-check` against the default three-model set.

To run the reasoning trap demo without Azure calls:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt reasoning-schedule
```

Live demo mode shows the current model and prompt while requests are running. If `ModelWorld:Azure:RequestTimeoutSeconds` is not configured, demo requests time out after 45 seconds per model.

For the static interactive selection flow, run:

```powershell
dotnet run --project src\ModelWorld -- --static
```

## Notes

- Use the [structured demo guide](docs/STRUCTURED_DEMO_GUIDE.md) for an instructor-led benchmark sequence, discussion prompts, and model-evaluation talking points.
- Static mode sends no Azure requests and requires no endpoint, deployment, token, or user secret.
- Connected mode uses keyless Microsoft Entra authentication only. API keys are not supported.
- The live client uses the OpenAI .NET package against the normalized Azure `/openai/v1/` endpoint with the `https://ai.azure.com/.default` token scope.
- Live token usage comes from Azure model responses. Live prices come from Azure Retail Prices API when resolvable; static mode uses local illustrative catalog prices.
- Costs are estimates. Azure Cost Management and invoices remain the source of truth for actual billed costs, discounts, credits, taxes, and marketplace terms.
- The static simulator and live Azure runner both implement `IModelRunner`, keeping the console flow independent of the execution source.