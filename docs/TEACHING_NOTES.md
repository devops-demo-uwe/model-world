# Teaching Notes

Use this file to capture model comparison observations that are useful for explaining AI model behavior to learners. Add a new dated note whenever a run reveals a pattern worth discussing.

## 2026-06-29: Reasoning Models Can Spend Hidden Completion Tokens

### Observation

In the summarization scenario, `o4-mini` produced a visible answer that was only moderately longer than the other models, but it reported far more completion tokens. This is a useful teaching example because `o4-mini` is a reasoning model.

### Teaching Point

Reasoning models are tuned to spend extra computation on multi-step problems before producing the final answer. They are best understood as deliberation-first models. General chat or instruction models usually try to answer directly, while reasoning models may use an internal reasoning budget to plan, check, or refine their response.

Those internal reasoning tokens may not appear in the visible answer, but they can still be counted as output or completion tokens by the model API. This means a reasoning model can look concise on screen while still using many more billable completion tokens than a non-reasoning model.

### Effect on Token Usage and Pricing

For non-reasoning chat models, completion tokens usually correspond closely to the visible response. For reasoning models, completion tokens can include both hidden reasoning work and the final visible answer.

That matters because output tokens are commonly billed separately from input tokens, and often at a higher rate. Hidden reasoning tokens can therefore increase cost even when the visible answer is short. They can also increase latency because the model is doing more work before returning the final answer.

### Effect on Results

Reasoning models are often a strong fit for tasks that benefit from careful intermediate thinking, such as math, logic, planning, debugging, constrained decisions, and multi-step analysis. They may be overkill for simple summarization, rewriting, or short product copy, where a general chat model can produce comparable output with lower latency and fewer completion tokens.

In Model World, this is a useful model-selection lesson: choose reasoning models when the task actually needs reasoning, not just because the model is more advanced.

### Discussion Prompt

When reviewing a comparison table, ask learners to compare visible answer quality against latency, completion tokens, and estimated cost. If the reasoning model used many more completion tokens without a noticeably better answer, ask whether the task justified the extra reasoning budget.