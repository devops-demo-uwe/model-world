# Teaching Notes

Use this file to save model comparison notes that help learners understand model behavior. Add a new dated note when a run shows a useful pattern.

## 2026-06-30: Release Note Brief Summarization Benchmark

### Prompt

Use the **Release Note Brief** prompt from the summarization catalog. The task asks the model to compress a product update into 45-60 words, 2-3 sentences, and include only user value, current limitation, and next milestone.

### Observation

This benchmark is not mainly testing obscure factual recall. It tests instruction-following under compression: can the model preserve the product point, obey the length and structure constraints, and sound useful to a stakeholder?

In one run, all models understood the main change and included the required themes. The strongest answer came from **GPT-5.4 mini** because it gave a concise stakeholder-ready summary: compare the same prompt across models, show response quality, latency, token usage, and estimated cost, note that pricing is illustrative, and name the next milestone as replacing estimates with actual billable cost data.

**GPT-5.4** was also correct, but a little heavier and more internal in tone. **Llama 3.3 70B Instruct** was fast, cheap, and usable, but compressed away some important product nuance, especially the broader value of comparing the same prompt across multiple models.

### Teaching Point

Use this example to show that model quality is not only about knowing the answer. For business-writing tasks, the useful question is whether the model can follow a brief, keep the required information, and compress without losing the product value.

This is also a good cost and latency lesson. Smaller or cheaper models can perform very well when the input is structured and the instructions are clear. For short summarization and release-note work, a cheaper model may be good enough, or even better, if it produces the clearest stakeholder-ready output.

### Suggested Scoring

Score this prompt on length control, user value, current limitation, next milestone, product language, and lack of noise. A practical rubric is:

| Model | Likely score | Why |
| --- | ---: | --- |
| GPT-5.4 mini | 9/10 | Best balance of concise, complete, stakeholder-ready output. |
| GPT-5.4 | 8/10 | Correct and detailed, but a little wordier. |
| Llama 3.3 70B Instruct | 7/10 | Efficient and usable, but loses some user-value framing. |

## 2026-06-30: Byzantine Inkstand Office General Knowledge Benchmark

### Prompt

```text
In Byzantine administrative history, what Greek title was used for the head of the imperial inkstand office?
```

### Expected Answer

The expected answer is **ἐπὶ τοῦ κανικλείου** / **epi tou kanikleiou**. Also accept **κανίκλειος** / **kanikleios**.

This official was responsible for the imperial inkstand and the special scarlet or purple ink used to approve imperial documents. The role may sound small, but it was connected to the emperor's written authority: documents, signatures, formulas, seals, and access to official approval.

### Compact Scoring Guidance

Give full credit for answers that name **epi tou kanikleiou** or **kanikleios**, including Greek-script forms. Give partial credit for descriptions such as "keeper of the imperial inkstand" when the title is missing. Do not give credit for broader office titles such as **asekretis**, **chartoularios**, **logothetes**, **protonotarios**, **sakellarios**, **megas logothetes**, or **praipositos**.

For strict matching, use this expected-answer shape:

```json
{
    "answer": "epi tou kanikleiou",
    "alternate_answers": [
        "ἐπὶ τοῦ κανικλείου",
        "kanikleios",
        "κανίκλειος",
        "epi tou kanikleiou / kanikleios"
    ]
}
```

### Why Models Give Different Answers

This prompt is useful because the answer is very obscure, but it does not require math or reasoning steps. Unless a model has a search or retrieval tool, it does not check an official list of Byzantine offices. It predicts an answer from patterns it learned during training: Byzantine government, court documents, secretaries, ink, imperial authority, and Greek titles. If the exact title appears clearly in those patterns, the model may answer **epi tou kanikleiou** or **kanikleios**. If the connection is weak, the model may still give an answer that sounds fluent and confident.

This is the hallucination risk in this benchmark. A hallucination is not always a completely invented answer. Sometimes it is a real term used in the wrong place. **Asekretis** and **chartoularios** are real Byzantine administrative terms, so they may sound believable, but they are too broad for this specific office. **Kanikleion** is also related to the inkstand, but it names the object or office context, not the official's title. These answers can sound correct because they are near the right topic, even when they miss the exact fact.

The model may not simply say "I do not know" because its default task is to produce a helpful-looking answer. It has learned that users usually expect an answer, and many training examples reward confident completion. Unless the prompt asks the model to state uncertainty, or the system uses retrieval and verification, the model may choose the most likely nearby answer instead of stopping at uncertainty.

Use this example to show that models may fill a knowledge gap with a likely nearby idea. In a model comparison, a confident but broad answer should be treated as partial topic knowledge, not as a precise correct answer.

## 2026-06-29: Verify the Rental Truck Math Locally

### Observation

The rental truck benchmark is easy to understand, but it still has several math traps. Included miles must be multiplied by the number of rental days. Extra miles apply only to Plan A. The coupon applies before tax. The insurance fee applies only to Plan B. Tax is added after discounts, mileage charges, and fees.

Model outputs can look confident and still have wrong numbers. Instructors should verify the expected answer with local arithmetic instead of trusting a model explanation by itself.

### Instructor Verification Script

Run the verifier from the repository root. It does not call Azure or any external service.

```powershell
python .\docs\scripts\rental_truck_verify.py
```

The script lives at [docs/scripts/rental_truck_verify.py](scripts/rental_truck_verify.py). It uses Python's `Decimal` type so money values round in a reliable way.

```python
from decimal import Decimal, ROUND_HALF_UP


def money(value: Decimal) -> Decimal:
	"""Round money for display using ordinary cents rounding."""
	return value.quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)
```

The full script includes notes so instructors can walk through each math step with learners.

### Expected Output

The script prints the calculation steps first. Then it prints the two-line answer expected from the benchmark prompt.

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

This benchmark is not hard because the story is obscure. It is hard because it combines several simple rules that are easy to mix up. A model can fail by using 100 included miles total instead of 100 per day, applying tax before the coupon, taxing only part of the subtotal, applying the insurance fee to the wrong plan, or rounding too early.

When reviewing live model outputs, ask learners to compare the two requested numbers with the script output. The goal is to show that a clear, confident answer is not the same as checked math.

## 2026-06-29: Why Math Is Hard for LLMs

### Observation

Large language models often sound clear and organized on math problems, but they can still return wrong numbers. The rental truck benchmark is a good classroom example because the story is familiar and the math is not advanced. Even so, small mistakes in order or bookkeeping change the final answer.

### Teaching Point

LLMs are mainly trained to predict likely text. They do not do arithmetic the same way a calculator or spreadsheet does. They learn many number patterns from examples, but the answer is still generated one token at a time. This means a model can write a calculation that looks convincing while losing an intermediate value, using the wrong order, or rounding at the wrong time.

A useful way to explain this is to compare remembered facts with computed answers. A model may correctly answer the distance from London to Paris because that fact appears often in text: travel pages, geography examples, tourism articles, flight-distance tables, and similar sources. In that case, the model is often repeating or rebuilding a familiar fact pattern.

That is different from calculating the distance between two small villages that rarely appear together in public text. A map system can look up coordinates, roads, traffic rules, ferries, borders, speed limits, and route options. Then it can run a route algorithm. An LLM without tools does not automatically do that. It may generate a plausible distance based on nearby place names or examples it has seen, but it has not necessarily measured anything.

The same difference appears in arithmetic. If the question is common, such as "What is 12 x 12?", the model may answer correctly because the pattern is very familiar. If the question has a new mix of discounts, fees, taxes, and included miles, the model has to track several intermediate values. Unless it uses a calculator, code interpreter, spreadsheet, or another deterministic tool, it is still producing text rather than running a guaranteed calculation.

Math problems are fragile because one early mistake affects the later steps. If a model treats Plan A as having 100 included miles total instead of 100 miles per day, every later value may look consistent but still be wrong. The final response can be neat and confident even when the calculation went off track.

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

For math-heavy tasks, ask the model to produce code or formulas that you can run. Do not ask it to calculate the final result only by generating text. A Python script, spreadsheet formula, SQL query, or small tested function can be run, checked, fixed, and reused. The model is still useful, but its role changes. It helps build the calculator instead of acting as the calculator.

This is the safer workflow to teach:

1. Ask the model to translate the word problem into explicit variables and deterministic code.
2. Run the code locally.
3. Inspect the intermediate values.
4. Compare the final answer to the model's natural-language answer.
5. Fix the code or assumptions if the intermediate values reveal a mismatch.

This difference matters. Token generation is good for explanations and code drafts. Deterministic tools are good for arithmetic. The best workflow uses both: use the model to express the calculation, then use code to run it.

## 2026-06-29: Reasoning Models Can Spend Hidden Completion Tokens

### Observation

In the summarization scenario, `o4-mini` produced a visible answer that was only a little longer than the other models. But it reported many more completion tokens. This is a useful teaching example because `o4-mini` is a reasoning model.

### Teaching Point

Reasoning models are tuned to spend more work on multi-step problems before they produce the final answer. They are best understood as models that think more before answering. General chat or instruction models usually answer more directly. Reasoning models may use extra hidden work to plan, check, or improve their response.

Those hidden reasoning tokens may not appear in the visible answer. But the model API can still count them as output or completion tokens. This means a reasoning model can look short on screen while still using many more billable completion tokens than a non-reasoning model.

### Effect on Token Usage and Pricing

For non-reasoning chat models, completion tokens usually match the visible response closely. For reasoning models, completion tokens can include both hidden reasoning work and the final visible answer.

This matters because output tokens are often billed separately from input tokens, and often at a higher price. Hidden reasoning tokens can increase cost even when the visible answer is short. They can also increase latency because the model is doing more work before it returns the final answer.

### Effect on Results

Reasoning models often fit tasks that need careful step-by-step thinking, such as math, logic, planning, debugging, constrained decisions, and multi-step analysis. They may be too much for simple summarization, rewriting, or short product copy. For those tasks, a general chat model may give similar quality with lower latency and fewer completion tokens.

In Model World, this is a useful model-selection lesson: choose reasoning models when the task needs reasoning, not only because the model is newer or more advanced.

### Discussion Prompt

When reviewing a comparison table, ask learners to compare visible answer quality with latency, completion tokens, and estimated cost. If the reasoning model used many more completion tokens without a clearly better answer, ask whether the task needed the extra reasoning work.

## 2026-06-29: Coding Review Reveals Implicit Contract Reasoning

### Prompt

```text
Review this C# method and suggest one improvement:
public static decimal Average(decimal total, int count) => total / count;
```

### Observation

All three models found the obvious bug: the method can divide by zero when `count` is `0`. This is a useful coding example because the interesting difference is not whether the models know C# syntax. The interesting difference is how well they understand the method's contract.

For a method named `Average`, a count of zero is invalid. A negative count is also almost certainly invalid. The better review is not only "avoid divide-by-zero." It is also "define `count` as a positive number."

### Model Comparison

| Model | Main fix | Exception type | Handles negative count? | Style |
| --- | --- | --- | --- | --- |
| GPT-5.4 | `count == 0` | `ArgumentException` | Mentions as optional | Nuanced reviewer |
| GPT-5.4 mini | `count == 0` | `ArgumentException` | No | Efficient assistant |
| o4-mini | `count <= 0` | `ArgumentOutOfRangeException` | Yes | Contract-focused reasoning |

`GPT-5.4` gave a solid review answer: fix the immediate bug, then mention that negative counts may also need to be rejected depending on the domain. `GPT-5.4 mini` gave a short practical answer: correct and likely good enough for many cases. `o4-mini` gave the strongest contract-level improvement by checking `count <= 0` and using `ArgumentOutOfRangeException`, which better describes an argument outside its allowed range.

### Stronger Review Target

```csharp
public static decimal Average(decimal total, int count)
{
    if (count <= 0)
    {
        throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
    }

    return total / count;
}
```

This is a compact way to explain the reasoning progression:

```text
surface bug: division by zero
deeper issue: invalid parameter range
better contract: count must be positive
```

### Teaching Point

This coding prompt is stronger than a simple trick question because it matches a realistic developer workflow. All models can find the visible bug. The best code-review answer also finds the hidden assumption: an average should not be computed from a non-positive count.

Use this example to separate fixing an error from improving a contract. The obvious bug is divide-by-zero. The better review says that `count` must be positive.

### Demo Takeaway

For a live presentation, summarize it this way:

> Here all models find the obvious defect. But `o4-mini` goes one step deeper: it treats the method as an API contract and checks the full invalid range, not only the divide-by-zero case. This is where reasoning-oriented models can be useful in code review.

The tradeoff is also visible in the benchmark table. `GPT-5.4 mini` is the practical default. `GPT-5.4` gives a more polished review. `o4-mini` may catch better edge-case logic, but it can be more verbose and may not be cheaper in a given run.
