# Teaching Notes

Use this file to capture model comparison observations that are useful for explaining AI model behavior to learners. Add a new dated note whenever a run reveals a pattern worth discussing.

## 2026-06-29: Verify the Rental Truck Math Locally

### Observation

The rental truck benchmark is intentionally easy to understand, but it still has several arithmetic traps: included miles must be multiplied by the number of rental days, extra miles apply only to Plan A, the coupon applies before tax, the insurance fee applies only to Plan B, and tax is added after discounts, mileage charges, and fees.

Because model outputs can look confident while still being numerically wrong, instructors should verify the expected answer with deterministic local arithmetic instead of relying on any model's explanation.

### Instructor Verification Script

Run the verifier from the repository root. It does not call Azure or any external service.

```powershell
python .\docs\scripts\rental_truck_verify.py
```

The script lives at [docs/scripts/rental_truck_verify.py](scripts/rental_truck_verify.py) and uses Python's `Decimal` type for deterministic money arithmetic.

```python
from decimal import Decimal, ROUND_HALF_UP


def money(value: Decimal) -> Decimal:
	"""Round money for display using ordinary cents rounding."""
	return value.quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)
```

The full script is intentionally annotated so instructors can walk through each arithmetic step with learners.

### Expected Output

The script prints the calculation flow first, then the two-line answer expected from the benchmark prompt.

```text
Rental Truck Choice benchmark verifier
=======================================
Rental length: 3 days
Planned distance: 486 miles
Sales tax: 8.2500%

Plan A
------
Included miles: 100 miles/day x 3 days = 300 miles
Extra miles: 486 planned - 300 included = 186 miles
Daily charge: 3 days x $79/day = $237.00
Extra-mile charge: 186 miles x $0.59/mile = $109.74
Coupon before tax: -$35.00
Subtotal before tax: $237.00 + $109.74 - $35.00 = $311.74
After tax: $311.74 x 1.0825 = $337.458550 -> $337.46

Plan B
------
Daily charge: 3 days x $109/day = $327.00
Insurance fee: $327.00 x 9.5% = $31.065 -> $31.07
Subtotal before tax: $327.00 + $31.065 = $358.065
After tax: $358.065 x 1.0825 = $387.6053625 -> $387.61

Comparison
----------
Plan A total: $337.46
Plan B total: $387.61
Savings: $387.6053625 - $337.458550 = $50.1468125 -> $50.15

Expected benchmark answer
-------------------------
Plan A total: $337.46
Cheaper plan: Plan A by $50.15
```

### Teaching Point

This benchmark is not hard because the story is obscure. It is hard because it combines several ordinary constraints that are easy to mishandle under pressure. A model can fail by using 100 included miles total instead of 100 per day, applying tax before the coupon, taxing only part of the subtotal, applying the insurance fee to the wrong plan, or rounding too early.

When reviewing live model outputs, have learners compare the two requested numbers against the script output. The goal is to teach that friendly, confident formatting is not the same thing as verified arithmetic.

## 2026-06-29: Why Math Is Hard for LLMs

### Observation

Large language models often sound fluent and organized on math problems, but they can still return wrong numbers. The rental truck benchmark is a good classroom example because the story is familiar and the required arithmetic is not advanced, yet small mistakes in sequencing or bookkeeping change the final answer.

### Teaching Point

LLMs are primarily trained to predict plausible text, not to execute arithmetic the way a calculator or spreadsheet does. They learn many numeric patterns from examples, but a generated answer is still produced token by token. That means the model can write a convincing-looking calculation while losing track of an intermediate value, applying an operation in the wrong order, or rounding at the wrong time.

A useful way to explain this is to contrast remembered facts with computed answers. A model may correctly answer the distance from London to Paris because that fact appears often in text it has seen: travel pages, geography examples, tourism articles, flight-distance tables, and similar sources. In that case, the model is often recalling or reconstructing a familiar fact pattern.

That is different from calculating the distance between two small villages that rarely appear together in public text. A mapping system such as Google Maps can look up coordinates, road networks, traffic rules, ferries, borders, speed limits, and route options, then run a path-finding algorithm. An LLM without tools does not automatically do that. It may generate a plausible-sounding distance based on nearby place names, regional scale, or examples it has seen, but it is not guaranteed to have measured anything.

The same distinction appears in arithmetic. If the question is common, such as "What is 12 x 12?", the model may answer correctly because the pattern is extremely familiar. If the question involves a new combination of discounts, fees, taxes, and included miles, the model has to maintain a chain of intermediate values. Unless it uses an actual calculator, code interpreter, spreadsheet, or other deterministic tool, it is still producing the answer as text rather than executing a guaranteed calculation.

Math problems are especially fragile because one early mistake propagates. If a model treats Plan A as having 100 included miles total instead of 100 miles per day, every later value may look internally consistent while still being wrong. The final response can be neatly formatted and confidently worded even though the calculation path drifted.

### Common Failure Modes

- **Bookkeeping mistakes:** The model misses which quantities apply to which plan, day, mile, fee, or discount.
- **Order-of-operations mistakes:** The model applies tax before a coupon, applies a fee after tax, or rounds before the final step.
- **Unit mistakes:** The model mixes per-day, per-mile, and total-trip values.
- **Arithmetic slips:** The model copies or computes an intermediate number incorrectly.
- **Format over correctness:** The model follows the requested answer shape while filling it with incorrect numbers.

### Discussion Prompt

Ask learners to separate three questions when judging a model answer:

1. Did the model understand the scenario?
2. Did it follow the requested output format?
3. Did the numbers match a deterministic calculation?

Those are different skills. A model may do the first two well and still fail the third. For production use, math-heavy workflows should use calculators, code, spreadsheets, symbolic tools, or validated business logic rather than trusting generated arithmetic by itself.

### Recommended Practice

For math-heavy tasks, ask the model to produce executable verification code rather than asking it to calculate the final result only through token generation. A Python script, spreadsheet formula, SQL query, or small unit-tested function can be run, inspected, corrected, and reused. The model is still useful, but its role shifts from being the calculator to helping build a calculator-shaped artifact.

This is the safer workflow to teach:

1. Ask the model to translate the word problem into explicit variables and deterministic code.
2. Run the code locally.
3. Inspect the intermediate values.
4. Compare the final answer to the model's natural-language answer.
5. Fix the code or assumptions if the intermediate values reveal a mismatch.

That distinction matters. Token generation is good at producing plausible explanations and code drafts. Deterministic tools are good at arithmetic. The best workflow combines both: use the model to help express the calculation, then use code to execute it.

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