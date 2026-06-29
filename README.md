# Model World

Model World is an early educational console prototype for comparing AI model behavior across representative prompts.

This first version does not connect to Azure AI Foundry. All model outputs, latency values, token counts, finish reasons, and costs are deterministic static examples so the app flow and console experience can be evaluated before live API integration.

## Prototype Contents

- 5 static model profiles, including a clearly labeled `GPT-5.5 (prototype sample)` entry.
- 5 prompt scenarios covering mathematics, reasoning, coding, summarization, and structured output.
- Comparison runs are limited to 3 models at a time so generated outputs fit in column-based result tables.
- Side-by-side result summaries with simulated latency, tokens, estimated cost, finish reason, and generated output.
- A Spectre.Console interface for readable tables and result panels.
- Unit tests for deterministic logic and catalog shape.

## Requirements

- .NET 10 SDK

## Run

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src\ModelWorld -- --demo
```

The demo compares the default three-model set: `GPT-5.5 (prototype sample)`, `GPT-4o`, and `GPT-4o mini`.

For the interactive selection flow, run:

```powershell
dotnet run --project src\ModelWorld
```

## Notes

- No Azure requests are sent by this prototype.
- No endpoint, deployment, API key, token, or user secret is required.
- Costs and speeds are illustrative estimates only, not billing guidance.
- The static simulator is intentionally isolated behind `IModelRunner` so a future Azure AI Foundry runner can replace it without rewriting the console flow.