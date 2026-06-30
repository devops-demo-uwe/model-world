# Structured Demo Guide

This guide is an instructor-led runbook for showing model evaluation with Model World. It turns the teaching notes into a live demo sequence: which benchmarks to run, why they are ordered that way, what tradeoffs to point out, and how to explain the model behavior learners will see.

The primary path is live Azure mode because it shows real responses, latency, token usage, finish reasons, and estimated cost. Live runs may incur Azure usage charges. Use static mode for rehearsal, offline rooms, screenshots, or no-cost walkthroughs.

Related references:

- [Azure setup](AZURE_SETUP.md) for live-mode prerequisites, deployment names, authentication, and cost controls.
- [Teaching notes](TEACHING_NOTES.md) for dated observations and scoring notes from previous runs.
- [Prompt catalog](../src/ModelWorld/Catalogs/PromptCatalog.cs) for the exact prompts and expected behavior.
- [Model catalog](../src/ModelWorld/Catalogs/ModelCatalog.cs) for model metadata, pricing hints, and behavior notes.

## Instructor Setup

Before class, verify the live setup once:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src\ModelWorld -- --demo --prompt structured-output
```

The last command sends live Azure requests to the default three-model comparison set: GPT-5.4 mini, o4-mini, and Llama 3.3 70B Instruct. It is a good smoke test because the prompt is short and the expected output is easy to inspect.

For a no-cost rehearsal of the same shape, add `--static`:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt structured-output
```

Keep the console table visible during discussion. The most important columns are output quality, latency, prompt tokens, completion tokens, total tokens, finish reason, and estimated cost. The point is not to crown one permanent winner. The point is to show that model choice depends on the task.

## Model Selection Lens

Before running prompts, spend two minutes on the model catalog table. It gives learners the three constraints they will keep revisiting during the demo: price, latency, and context window.

| Model | Catalog context window | Catalog typical latency | Local price / 1M tokens |
| --- | ---: | ---: | ---: |
| GPT-5.4 | 256K tokens | 3,850 ms | $3.00 input / $12.00 output |
| GPT-5.4 mini | 128K tokens | 1,150 ms | $0.60 input / $2.40 output |
| o4-mini | 128K tokens | 2,050 ms | $1.10 input / $4.40 output |
| DeepSeek-V4-Pro | 128K tokens | 2,550 ms | $0.50 input / $1.50 output |
| Llama 3.3 70B Instruct | 128K tokens | 2,700 ms | $0.80 input / $0.80 output |

How to explain the terms:

- **Price** is the unit rate per million input and output tokens. The run-level estimated cost is price multiplied by prompt and completion tokens. In live mode, Model World prefers Azure Retail Prices API rates when it can match input and output meters confidently; in static mode, it uses the local catalog prices above. Treat both as teaching estimates.
- **Latency** is the elapsed time for a model call. The catalog latency is a rough expectation, while the result table shows the measured live or simulated elapsed time for this specific prompt. Network conditions, throttling, regional load, prompt length, output length, and hidden reasoning work can all change it.
- **Context window** is the maximum prompt, conversation history, retrieved content, tool output, and generated answer the model can fit at once. A larger context window is useful for long documents and long-running chats, but it does not guarantee better reasoning. Filling a large context window also increases prompt tokens, cost, and often latency.

Instructor story: ask learners to make a model-selection sentence after each run: "For this task, I would choose `<model>` because its answer was good enough, its latency was acceptable, its price fits the expected volume, and its context window is large enough for the real workload." This turns the demo from a beauty contest into an engineering tradeoff.

## Recommended Demo Arc

Run the benchmarks in this order:

| Order | Prompt id | Title | Main lesson |
| ---: | --- | --- | --- |
| 1 | `math-check` | Rental Truck Choice | Confidence is not correctness; deterministic verification matters. |
| 2 | `structured-output` | JSON Task Extractor | Machine-readable format discipline can matter more than eloquence. |
| 3 | `summarization` | Release Note Brief | Smaller models can win bounded business-writing tasks. |
| 4 | `reasoning-schedule` | Bat and Ball Trap | Reasoning models are useful when the task rewards checking the trap. |
| 5 | `coding-review` | C# Guard Clause | Good review moves from surface bugs to API contract reasoning. |
| 6 | `general-knowledge-escalation` | Easy to Obscure Recall | Hallucination often looks like confident nearby-topic completion. |

This arc starts with results learners can verify quickly, then moves into application constraints, cost-quality tradeoffs, latency and hidden reasoning work, code judgment, and finally factual uncertainty. Context window is not stressed directly by these short prompts; use it as a sizing discussion for the real workload each prompt represents.

## Demo Paths

For a 15-minute live demo, run these three prompts:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt math-check
dotnet run --project src\ModelWorld -- --demo --prompt structured-output
dotnet run --project src\ModelWorld -- --demo --prompt general-knowledge-escalation
```

This gives a compact story: math can be wrong, JSON must be parseable, and obscure facts can trigger confident hallucination.

For a 45-minute classroom walkthrough, run all six prompts in the recommended order:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt math-check
dotnet run --project src\ModelWorld -- --demo --prompt structured-output
dotnet run --project src\ModelWorld -- --demo --prompt summarization
dotnet run --project src\ModelWorld -- --demo --prompt reasoning-schedule
dotnet run --project src\ModelWorld -- --demo --prompt coding-review
dotnet run --project src\ModelWorld -- --demo --prompt general-knowledge-escalation
```

For static rehearsal, use the same commands with `--static` after `--`:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt math-check
```

To choose different models or multiple prompts interactively, run:

```powershell
dotnet run --project src\ModelWorld
```

The interactive flow limits comparisons to three models at a time so outputs remain readable.

## Benchmark 1: Rental Truck Choice

Live run:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt math-check
```

Static rehearsal:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt math-check
```

What it reveals: everyday arithmetic is fragile for language models. The story is easy, but the model must track included miles per day, extra-mile charges, a coupon before tax, an insurance fee on only one plan, tax order, rounding, and a strict two-line output format.

What to look for:

- The final Plan A total should be `$337.46`.
- Plan A should be cheaper by `$50.15`.
- A response can be fluent, formatted correctly, and still have wrong numbers.
- Check whether a model uses 100 included miles total instead of 100 miles per day.
- Watch for early rounding, tax applied before the coupon, or insurance applied to the wrong plan.

Technical background: an LLM generates likely text tokens. It has seen many arithmetic explanations, invoices, coupons, taxes, and comparison answers, but it is not automatically running a calculator. A small bookkeeping error early in the generated chain can make every later line look internally consistent and still be wrong.

Instructor story: ask learners to separate three skills: understanding the scenario, obeying the output format, and computing the answer. A model can succeed at the first two and fail the third.

Tradeoff lens: this prompt is short, so context window does not matter much. Price and latency only matter after correctness; the fastest or cheapest wrong arithmetic answer is not useful.

Verification option:

```powershell
python .\docs\scripts\rental_truck_verify.py
```

Use the local verifier when learners ask which answer is correct. The script sends no Azure requests.

## Benchmark 2: JSON Task Extractor

Live run:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt structured-output
```

Static rehearsal:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt structured-output
```

What it reveals: structured output is not about sounding smart. It is about obeying a contract that downstream software can parse.

What to look for:

- Output should be valid JSON with exactly `priority`, `owner`, and `nextAction`.
- There should be no markdown fence, explanation, greeting, or extra fields.
- The owner should be Erin, priority should be high, and the action should be validating the demo run before Friday.
- The best model may be the cheapest reliable one, not the most elaborate one.

Technical background: models are often trained to be conversational, so they may add helpful framing unless the instruction is clear and the model has strong format discipline. In application workflows, one extra sentence can break automation even when the semantic answer is correct.

Instructor story: contrast human usefulness with machine usefulness. A polite explanation may be nice in chat, but bad in a parser. This is a clean place to introduce schema validation and structured-output tests.

Tradeoff lens: if several models return valid JSON, prefer the cheapest and lowest-latency option that is reliable under repeated runs. Context window matters only when the extractor will receive long emails, tickets, transcripts, or retrieved records.

## Benchmark 3: Release Note Brief

Live run:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt summarization
```

Static rehearsal:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt summarization
```

What it reveals: model quality is not only about broad intelligence. For a bounded business-writing task, clarity, compression, and audience fit often matter more than raw capability.

What to look for:

- Does the answer stay within 45-60 words and 2-3 sentences?
- Does it include user value, current limitation, and next milestone?
- Does it avoid internal implementation noise?
- Compare whether the smaller model gives a cleaner stakeholder-ready result with lower latency or cost.

Technical background: clear constraints narrow the answer space. When the prompt defines the audience, length, tone, and required content, compact models often have enough signal to produce a strong answer. Larger models may add nuance, but that can become noise when the task is deliberately small.

Instructor story: ask learners whether they would ship the answer, not which model sounds most impressive. This prompt is a good antidote to choosing the biggest model by default.

Tradeoff lens: summarization is where context window becomes concrete. This demo uses a small input, but real release-note workflows may include long issue lists, PR summaries, incident notes, or customer feedback. A bigger context window can reduce preprocessing, while a compact model may still be the better price-latency fit when the source text is bounded.

## Benchmark 4: Bat and Ball Trap

Live run:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt reasoning-schedule
```

Static rehearsal:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt reasoning-schedule
```

What it reveals: some prompts reward deliberate checking. The tempting answer is 10 cents, but the correct answer is 5 cents because `$0.05 + $1.05 = $1.10`.

What to look for:

- Does the model return 5 cents or the tempting 10 cents answer?
- Does it verify the algebra or only provide a confident short answer?
- Does a reasoning model use more completion tokens than the visible answer suggests?
- Did higher cost or latency buy a better result for this task?

Technical background: reasoning-oriented models may spend extra hidden work before producing the final response. Those hidden reasoning tokens may be counted as completion tokens even when the visible answer is short. This can increase both latency and estimated cost.

Instructor story: this is where the phrase "use the right model for the job" becomes concrete. If extra reasoning catches the trap, the extra cost may be justified. If the task is simple extraction or copywriting, that same extra work may be waste.

Tradeoff lens: reasoning work often shows up as higher latency and higher completion-token cost. Use this prompt to ask whether the extra spend bought a materially better answer, or whether a deterministic calculator would be cheaper and more reliable.

## Benchmark 5: C# Guard Clause

Live run:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt coding-review
```

Static rehearsal:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt coding-review
```

What it reveals: code review is partly bug detection and partly contract reasoning. The obvious defect is division by zero, but the deeper contract is that `count` must be positive.

What to look for:

- Does the model catch `count == 0`?
- Does it also reject negative counts with `count <= 0`?
- Does it recommend an appropriate exception such as `ArgumentOutOfRangeException`?
- Does it focus on the one important improvement instead of style nits?

Technical background: code models often recognize common bug patterns. Better review behavior also reasons about the API boundary: what inputs make sense for this method, what invariant should be enforced, and how the method should fail when the contract is violated.

Instructor story: draw the ladder on the board: surface bug, invalid range, better contract. This helps learners see why two correct-looking reviews can differ in usefulness.

Tradeoff lens: context window matters more in real code review than in this tiny snippet. Reviewing a full file, diff, test failure, or architecture note consumes prompt tokens. A larger window may help fit the evidence, but price and latency can climb quickly if every pull request sends thousands of tokens.

## Benchmark 6: Easy to Obscure Recall

Live run:

```powershell
dotnet run --project src\ModelWorld -- --demo --prompt general-knowledge-escalation
```

Static rehearsal:

```powershell
dotnet run --project src\ModelWorld -- --static --demo --prompt general-knowledge-escalation
```

What it reveals: factual recall is uneven. A model may answer common facts correctly, specialized facts sometimes, and obscure facts with confident but wrong nearby terms.

What to look for:

- Easy answer: Mars.
- Hard answer: Henri Moissan.
- Obscure answer: `epi tou kanikleiou` or `kanikleios`.
- Wrong but plausible Byzantine titles such as `asekretis`, `chartoularios`, or `logothetes` should not receive full credit.
- Watch for confidence that does not match evidence.

Technical background: without browsing or retrieval, the model is not checking an authoritative source. It predicts a likely completion from patterns in its training data. When the exact fact is weakly represented, the model may choose a real term from the same topic area. That is still a hallucination if it answers the wrong question.

Why models often do not say "I do not know": many chat models are optimized to be helpful and complete the user's request. Training data and feedback often reward useful-looking answers. Unless the prompt asks for uncertainty, the system includes a refusal/abstention policy, or retrieval supplies grounding evidence, the model may prefer the most likely nearby answer over stopping.

Instructor story: explain that hallucination is not always nonsense. Sometimes it is a fluent answer built from real neighboring facts. That is why fact-heavy workflows need retrieval, citations, domain validation, or human review.

Tradeoff lens: paying more or waiting longer does not guarantee obscure factual recall. Context window becomes valuable when paired with retrieval, because the model can read authoritative source material instead of relying only on memorized patterns.

## Cross-Cutting Discussion Prompts

Use these after any run:

1. Did the model answer the right question?
2. Did it follow the requested format?
3. Is the answer correct against an external check or rubric?
4. Was the measured latency acceptable for the user experience?
5. Did the price and token usage change the model-selection decision at expected volume?
6. Is the model's context window large enough for the real prompt, history, retrieved evidence, and answer?
7. Would this task benefit from retrieval, a calculator, schema validation, tests, or human review?

## What To Watch In The Results Table

- **Quality:** correctness, completeness, audience fit, and usefulness for the task.
- **Formatting:** strict JSON, requested line count, requested fields, and no extra prose.
- **Latency:** whether this measured run is fast enough for the user experience. Compare it with the catalog's typical latency, but do not treat one run as a permanent speed ranking.
- **Prompt tokens:** how much context the task consumes before generation starts. Long documents, chat history, retrieval snippets, and tool output all increase this number.
- **Completion tokens:** visible answer tokens plus any hidden reasoning tokens reported by the service. Output tokens are often priced differently from input tokens.
- **Estimated cost:** the run-level token estimate after applying available price data. Useful for comparison, not a billing guarantee.
- **Finish reason:** whether the model stopped normally, hit a limit, or ran into filtering.
- **Context window:** shown in the model catalog rather than the result table. Use it to decide whether the model can fit the real workload, not just the short classroom prompt.

## Cost And Safety Notes

Live mode sends Azure requests. Keep classroom comparisons small, especially when every student is running the same commands. The default scripted demo sends three model calls per prompt. Running all six prompts against the default comparison set sends eighteen model calls.

Treat costs as estimates. The app combines token usage with pricing data when available, but actual billing can differ because of region, discounts, credits, taxes, marketplace terms, price changes, and related services. For a real application, multiply per-run cost by expected traffic and include adjacent services such as retrieval, storage, monitoring, and hosting.

Do not treat context window as free capacity. Large prompts may fit, but they still consume input tokens, can add latency, and can make evaluation harder if the model has to sift through irrelevant evidence.

Preserve content-filter, refusal, and uncertainty outcomes during demos. They are not just errors; they are part of model behavior. Use them to discuss safety policy, prompt framing, and application design.

## Closing Takeaways

- There is no best model in the abstract. There is a best fit for a task, budget, latency target, and risk tolerance.
- Correct-looking text is not the same as verified truth.
- Cheap and fast models can be excellent when the task is bounded and the prompt is clear.
- Reasoning models are valuable when the task rewards careful checking, but hidden completion tokens can raise cost.
- Context windows are capacity planning, not intelligence scores. They matter when the real workload includes long inputs, retrieved evidence, or extended conversation history.
- Latency is a product constraint. A better answer may still be the wrong choice if users cannot wait for it.
- Hallucinations can sound precise because they often draw from real neighboring facts.
- Production systems should pair models with deterministic tools, retrieval, validation, tests, monitoring, and human review where the risk requires it.