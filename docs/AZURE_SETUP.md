# Azure AI Foundry Setup for Instructors

Model World is designed to run in connected Azure mode by default. Live mode uses Microsoft Entra ID through `DefaultAzureCredential` only. API keys are intentionally unsupported.

A free static mode remains available with `--static` for offline demos, development, and CI-style validation.

## What Live Mode Uses

The first live integration targets the Foundry/OpenAI v1 chat completions endpoint:

```text
https://<resource-name>.openai.azure.com/openai/v1/
```

Some Foundry resources also expose this form:

```text
https://<resource-name>.services.ai.azure.com/openai/v1/
```

You can configure the app with the base resource endpoint, and it will append `/openai/v1/` if needed.

## Prerequisites

- An Azure subscription.
- Permission to create or use an Azure AI Foundry or Azure OpenAI resource.
- Azure CLI installed for local instructor machines.
- .NET 10 SDK.
- The deployed chat models used by the catalog.
- Microsoft Entra access to invoke the model deployment.

## Create the Foundry Resource and Deployments

1. Open Azure AI Foundry.
2. Create or select a project/resource in a supported region.
3. Deploy the five classroom comparison models.
5. Record the deployment names.

The app catalog currently uses these default deployment names:

| Model profile | Default deployment name |
| --- | --- |
| GPT-5.4 | `gpt-5.4` |
| GPT-5.4 mini | `gpt-5.4-mini` |
| o4-mini | `o4-mini` |
| DeepSeek-V4-Pro | `DeepSeek-V4-Pro` |
| Llama 3.3 70B Instruct | `Llama-3.3-70B-Instruct` |

If your deployment names differ, use deployment overrides in configuration instead of editing source.

## Configure Keyless Authentication

Sign in locally with Azure CLI:

```powershell
az login
az account set --subscription "<subscription-id-or-name>"
```

Assign the instructor or lab identity permission to invoke the model. For Azure OpenAI in Foundry, use the least-privilege role available for inference, commonly `Cognitive Services OpenAI User`.

```powershell
az role assignment create `
  --assignee "<user-or-service-principal-object-id>" `
  --role "Cognitive Services OpenAI User" `
  --scope "<azure-ai-resource-id>"
```

If your resource uses a Foundry model endpoint role instead, assign the equivalent model invocation role at the Foundry project/resource scope. The important requirement is that `DefaultAzureCredential` can acquire a token for `https://ai.azure.com/.default` and the identity is authorized to call the deployment.

Do not configure API keys. Do not set `AZURE_OPENAI_API_KEY` for this app.

## Configure Model World

Use user secrets for local instructor machines:

```powershell
dotnet user-secrets set "ModelWorld:Azure:Endpoint" "https://<resource-name>.openai.azure.com/openai/v1/" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:MaxOutputTokenCount" "300" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:Temperature" "0.2" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:RequestTimeoutSeconds" "120" --project src\ModelWorld\ModelWorld.csproj
```

Live mode uses each model's catalog `PricingRegion` to query Azure Retail Prices API once at startup. The current classroom catalog uses `swedencentral`. If a confident API meter is unavailable, the startup catalog uses the local catalog fallback price and flags the row. If an API meter is found but differs from the local catalog fallback, the API price is displayed and the row is flagged. If you need to point at a test or proxy endpoint for pricing lookup, set `ModelWorld:Azure:PricingEndpoint`; otherwise keep the default public endpoint.

If your deployment names differ from the catalog defaults, add overrides:

```powershell
dotnet user-secrets set "ModelWorld:Azure:DeploymentOverrides:gpt-54" "<your-gpt-5.4-deployment>" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:DeploymentOverrides:gpt-54-mini" "<your-gpt-5.4-mini-deployment>" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:DeploymentOverrides:o4-mini" "<your-o4-mini-deployment>" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:DeploymentOverrides:deepseek-v4-pro" "<your-deepseek-deployment>" --project src\ModelWorld\ModelWorld.csproj
dotnet user-secrets set "ModelWorld:Azure:DeploymentOverrides:llama-33-70b-instruct" "<your-llama-deployment>" --project src\ModelWorld\ModelWorld.csproj
```

For lab VMs, containers, or CI-style environments, use environment variables with double underscores:

```powershell
$env:ModelWorld__Azure__Endpoint = "https://<resource-name>.openai.azure.com/openai/v1/"
$env:ModelWorld__Azure__MaxOutputTokenCount = "300"
$env:ModelWorld__Azure__Temperature = "0.2"
$env:ModelWorld__Azure__RequestTimeoutSeconds = "120"
$env:ModelWorld__Azure__DeploymentOverrides__gpt-54-mini = "<your-gpt-5.4-mini-deployment>"
```

`DefaultAzureCredential` can use Azure CLI sign-in, managed identity, Visual Studio credentials, or environment-based workload identity. Prefer Azure CLI for instructor laptops and managed identity for hosted lab infrastructure.

## Run a Smoke Test

Static demo mode is always free and sends no Azure requests:

```powershell
dotnet run --project src\ModelWorld -- --static --demo
```

To compare the bat-and-ball reasoning trap offline:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt reasoning-schedule
```

Connected demo mode sends Azure requests and may incur charges:

```powershell
dotnet run --project src\ModelWorld -- --demo
```

The connected demo runs one prompt against the default three-model set: `gpt-5.4-mini`, `o4-mini`, and `Llama-3.3-70B-Instruct`.
While requests are running, the status line shows the current model and prompt. If `ModelWorld:Azure:RequestTimeoutSeconds` is not configured, demo requests time out after 45 seconds per model.

## Classroom Cost Controls

- Keep `--static` for walkthroughs that do not need live model behavior.
- The default run path sends real Azure requests.
- Keep comparisons small. The app limits interactive comparisons to 3 models.
- A run with 3 models and 5 prompts sends 15 billable model calls.
- Prefer `gpt-4o-mini` or another low-cost model for first exercises.
- Review the startup model table before class. Live mode labels rates from Azure Retail Prices API when the meters can be matched; otherwise it shows pricing unavailable instead of guessing.
- Use Azure Cost Management and your invoice for actual billed costs. Retail pricing estimates do not include negotiated discounts, credits, taxes, private marketplace terms, or every regional billing nuance.
- Set Azure budgets, quota limits, and deployment rate limits where available.
- Remove or avoid expensive deployments unless they are part of the lesson.

## Troubleshooting

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| Live mode says endpoint is missing | `ModelWorld:Azure:Endpoint` is not configured | Set user secrets or environment variables |
| Authentication failed | Azure CLI is not signed in or the identity lacks access | Run `az login`, check subscription, assign the inference role |
| Deployment not found | Catalog deployment name does not match Azure | Add a `DeploymentOverrides` setting or rename the Azure deployment to the catalog default |
| Catalog fallback pricing is flagged | Azure Retail Prices API did not expose a confident input/output meter match for the model, region, or deployment type | Check the model catalog `PricingRegion`, verify the model's Azure pricing page or Foundry pricing terms, and treat Cost Management as the billing source of truth |
| API/catalog price mismatch is flagged | Azure Retail Prices API returned a confident meter, but it differs from the local catalog fallback | Prefer the displayed API price for the run, then update the catalog if the API price reflects the intended deployment type |
| Content filter finish reason | Azure content filtering blocked output | Treat it as a learning result and discuss safety behavior |
| Timeout | Network, quota, or model latency issue | Increase `RequestTimeoutSeconds` or try a smaller model |

## Live Tests

Normal `dotnet test` must stay offline and free. If you add future live integration tests, gate them behind an explicit opt-in environment variable and a dedicated low-cost deployment.
