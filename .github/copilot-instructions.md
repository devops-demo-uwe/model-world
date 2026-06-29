# Copilot Instructions for Model World

## Project Intent

Model World is an educational C# console application for developers who are starting to learn AI agent and model development. The app should help users compare a curated set of Microsoft Azure AI Foundry models across representative prompts, with clear side-by-side visibility into output quality, latency, token usage, and estimated cost.

The goal is not to build a production benchmark harness. The goal is to create a practical learning tool that makes model behavior understandable through small, repeatable experiments.

## Platform and Stack

- Build with the latest .NET LTS available for this project date: .NET 10, targeting `net10.0` unless the user explicitly changes the target.
- Use modern C# style: nullable reference types enabled, file-scoped namespaces, async APIs for network work, and small cohesive types.
- Implement as a console app first. Do not add a web UI unless the user explicitly asks.
- Prefer built-in .NET libraries and stable Azure SDK packages. Add dependencies only when they materially improve the app.
- For rich console UI, prefer `Spectre.Console` for tables, panels, prompts, progress displays, ANSI color, and box drawing.
- For Azure authentication, prefer Microsoft Entra ID with `Azure.Identity` and `DefaultAzureCredential`. Support API-key authentication only through environment variables or user secrets, never hard-coded values.

## Azure AI Foundry Integration

- Connect to Azure AI Foundry model endpoints using current Microsoft guidance.
- For model inference/chat completions, use the Azure SDK surface that matches the endpoint type:
  - Azure AI Inference SDK patterns such as `Azure.AI.Inference` and `ChatCompletionsClient` for Foundry model inference endpoints.
  - Azure OpenAI SDK patterns such as `Azure.AI.OpenAI` / OpenAI chat clients when targeting Azure OpenAI deployments in Foundry.
- Keep endpoint, deployment/model names, API versions, and auth mode configurable through options rather than scattering constants through the code.
- Never commit secrets, keys, tokens, connection strings, or real endpoint-specific credentials.
- Handle Azure request failures clearly, including authentication failures, content filtering, rate limits, unavailable deployments, and malformed configuration.
- Record service metadata that is available from responses, especially prompt tokens, completion tokens, total tokens, finish reason, and request duration.

## Educational Benchmark Design

- Treat comparisons as illustrative rather than scientifically definitive.
- Make the curated model list intentionally small and diverse. Favor models that show meaningful tradeoffs in capability, speed, context support, reasoning behavior, and cost.
- Store model metadata separately from execution code. Include fields such as display name, provider/family, deployment name, capability notes, context window if known, strengths, limitations, pricing notes, and recommended use cases.
- Store prompt gallery entries separately from execution code. Include fields such as domain, title, prompt text, intent, expected behavior, and what the prompt is meant to reveal.
- Include prompt domains such as general knowledge, mathematics, reasoning, coding, summarization, instruction following, structured output, creativity, and safety-aware refusal behavior.
- Keep prompts short enough to run cheaply, but rich enough to show meaningful differences.
- For math and reasoning prompts, prefer prompts with checkable answers or clear rubrics.

## Console UX Direction

- The app should feel polished, technical, and readable in a terminal.
- Assume a Nerd Font may be present, but always keep the interface understandable without relying solely on special glyphs.
- Use colored output, tables, panels, box drawing, progress indicators, and tasteful ASCII art where it improves comprehension.
- Present model statistics in compact comparison tables before users run prompts.
- Present each run with model name, prompt title/domain, elapsed time, token usage, estimated cost when available, finish reason, and generated output.
- Make side-by-side comparisons easy to scan. Use consistent ordering, column names, units, and color conventions.
- Do not let decorative console output obscure the actual educational content.
- Include a plain or low-color mode if practical, especially for CI logs or terminals without full ANSI support.

## Cost, Speed, and Token Reporting

- Measure latency with `Stopwatch` around the service call.
- Use token usage returned by the service when available. Do not invent token counts when the service does not return them.
- Estimate cost only when pricing metadata is configured and clearly label it as an estimate.
- Keep cost calculation logic isolated and testable.
- Report units explicitly, for example milliseconds, prompt tokens, completion tokens, total tokens, input cost, output cost, and total estimated cost.

## Architecture Preferences

- Separate concerns into focused areas:
  - Configuration loading and validation.
  - Azure client creation and authentication.
  - Model catalog metadata.
  - Prompt gallery metadata.
  - Prompt execution and result collection.
  - Cost and token accounting.
  - Console rendering.
- Prefer dependency injection only when it simplifies testing or composition. Avoid enterprise ceremony for a small console app.
- Keep domain records simple and serializable so catalogs can move to JSON/YAML later if helpful.
- Avoid global mutable state except for process-level console configuration.
- Design execution so a single prompt can run across multiple selected models, and one model can run across multiple selected prompts.

## Testing Expectations

- Add unit tests for deterministic logic such as cost calculation, result formatting helpers, configuration validation, and prompt/model catalog parsing.
- Do not require live Azure calls for normal unit tests.
- Keep live Azure tests opt-in and clearly marked, using environment variables for endpoint and deployment configuration.
- When changing Azure integration code, prefer adding a fake or adapter boundary so behavior can be tested without sending real requests.

## Coding Style

- Keep names explicit and beginner-friendly. This project is educational, so clarity beats clever compression.
- Prefer small methods and records over large procedural blocks.
- Use exceptions for unexpected programmer/configuration errors and friendly result messages for expected user-facing failures.
- Use `CancellationToken` for network operations and long-running prompt batches.
- Avoid one-letter variable names.
- Do not add comments that repeat the code. Add comments only when they explain a non-obvious design choice or Azure-specific nuance.

## Documentation Expectations

- Keep setup instructions current with the selected .NET version and Azure SDK packages.
- Document required Azure resources, roles, and environment variables before adding features that depend on them.
- Explain how to run a cheap demo safely, including how many model calls it makes.
- Warn users when a command may incur Azure usage charges.

## Security and Responsible AI

- Never log secrets or raw credentials.
- Avoid collecting or storing personal data in sample prompts or default catalogs.
- Make it clear that model outputs may be incorrect and should be evaluated critically.
- Include safe, educational prompt examples. Do not add prompts that request harmful, hateful, exploitative, or illegal content.
- Preserve and display content-filter and refusal outcomes as part of the learning experience instead of treating them as generic failures.

## Default Commands

When the .NET project exists, prefer these validation commands:

```powershell
dotnet restore
dotnet build
dotnet test
```

For live Azure checks, use explicit opt-in commands or environment flags so regular validation does not spend money.