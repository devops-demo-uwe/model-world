# Model World

Model World is an educational console prototype for comparing AI model behavior across representative prompts.

The app supports two modes:

- Live Azure mode, the default, calls Azure AI Foundry/OpenAI v1 chat completions with Microsoft Entra ID keyless authentication.
- Static mode, enabled with `--static`, uses deterministic sample outputs and sends no Azure requests.

## Prototype Contents

- 5 configured model profiles for the deployed classroom comparison set: `gpt-5.4`, `gpt-5.4-mini`, `o4-mini`, `DeepSeek-V4-Pro`, and `Llama-3.3-70B-Instruct`.
- 5 prompt scenarios covering mathematics, reasoning, coding, summarization, and structured output.
- Comparison runs are limited to 3 models at a time so generated outputs fit in column-based result tables.
- Side-by-side result summaries with latency, tokens, estimated cost, finish reason, and generated output.
- A Spectre.Console interface for readable tables and result panels.
- Unit tests for deterministic logic and catalog shape.
- Keyless-only Azure integration through `DefaultAzureCredential`.

## Requirements

- .NET 10 SDK
- Azure CLI or another `DefaultAzureCredential` source, an Azure AI Foundry/OpenAI resource, deployed chat models, and an Entra role assignment that can invoke those deployments.

## Configure Azure Access

Live mode is the normal operating mode and may incur Azure usage charges.

```powershell
az login
dotnet user-secrets set "ModelWorld:Azure:Endpoint" "https://<resource-name>.openai.azure.com/openai/v1/" --project src\ModelWorld\ModelWorld.csproj
```

The default catalog expects these deployment names exactly:

| Model | Expected deployment name |
| --- | --- |
| GPT-5.4 | `gpt-5.4` |
| GPT-5.4 mini | `gpt-5.4-mini` |
| o4-mini | `o4-mini` |
| DeepSeek-V4-Pro | `DeepSeek-V4-Pro` |
| Llama 3.3 70B Instruct | `Llama-3.3-70B-Instruct` |

If your Azure deployment names differ, configure deployment overrides as described in the instructor setup guide.

## Run Connected Mode

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src\ModelWorld -- --demo
```

The demo sends live Azure requests for the default three-model set: `gpt-5.4-mini`, `o4-mini`, and `Llama-3.3-70B-Instruct`.

For the interactive selection flow, run:

```powershell
dotnet run --project src\ModelWorld
```

## Run Static Mode

Static mode is kept as an offline fallback for demos, CI-style screenshots, or development without Azure access.

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src\ModelWorld -- --static --demo
```

For the static interactive selection flow, run:

```powershell
dotnet run --project src\ModelWorld -- --static
```

## Notes

- Static mode sends no Azure requests and requires no endpoint, deployment, token, or user secret.
- Connected mode uses keyless Microsoft Entra authentication only. API keys are not supported.
- Costs are estimates based on configured catalog pricing. Treat them as learning aids, not billing guidance.
- The static simulator and live Azure runner both implement `IModelRunner`, keeping the console flow independent of the execution source.