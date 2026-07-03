# Foundry Grounded QA Evaluation Demo

This guide shows how to use the evaluator-friendly JSONL dataset with the official Azure AI Foundry evaluation page. It is intentionally separate from the Model World console demo because this dataset is built for Foundry's generic evaluators, not for the classroom prompt-comparison scenarios.

Related files:

- [data/foundry-grounded-qa-evaluation.jsonl](data/foundry-grounded-qa-evaluation.jsonl) is the recommended upload file for this demo.
- [data/model-world-foundry-evaluation.jsonl](data/model-world-foundry-evaluation.jsonl) contains the Model World teaching scenarios and is better for manual prompt comparison.

Microsoft references:

- [Run evaluations in the cloud by using the Microsoft Foundry SDK](https://learn.microsoft.com/azure/foundry/how-to/develop/cloud-evaluation)
- [Built-in evaluators reference](https://learn.microsoft.com/azure/foundry/concepts/built-in-evaluators)

## What This Demo Measures

The grounded QA dataset uses fictional Trailhead Outfitters support policies. Each row asks a short customer-service question and provides the policy context needed to answer it.

This is a better fit for Foundry built-in evaluators because the fields are simple:

| Field | Purpose |
| --- | --- |
| `case_id` | Stable row identifier for discussion and result filtering. |
| `topic` | Dataset category such as returns, shipping, rentals, repairs, membership, or sustainability. |
| `query` | The customer question sent to the model. |
| `context` | The source text the model should use. |
| `ground_truth` | The expected reference answer. |

The dataset intentionally has no `response` column. In a **Target: Model** evaluation, Foundry calls each configured model and uses the generated answer as the response.

## Recommended Portal Flow

1. In Azure AI Foundry, open **Evaluations**.
2. Select **Create new evaluation**.
3. Choose **Target: Model**.
4. Select the model deployments you want to compare.
5. In **Data**, choose **Existing dataset**, then **Upload new dataset**.
6. Upload [data/foundry-grounded-qa-evaluation.jsonl](data/foundry-grounded-qa-evaluation.jsonl).
7. In **Configure models**, configure every selected deployment with the same prompt template.
8. In **Criteria**, choose the evaluators listed below.
9. In **Review**, confirm the dataset, model deployments, and criteria before submitting.

Live evaluations send requests to your selected deployments and may incur Azure usage charges.

## Synthetic Data Option

If you choose **Synthetic generation** instead of uploading [data/foundry-grounded-qa-evaluation.jsonl](data/foundry-grounded-qa-evaluation.jsonl), use the prompt configuration to describe the dataset you want Foundry to create.

In the **Add custom prompt** dialog, paste this into the **System** field:

```text
Generate realistic customer questions about camping and outdoor gear. Focus on common camping topics such as tents, sleeping bags, camp stoves, lanterns, coolers, water filters, hiking boots, campground rules, packing lists, returns, rentals, repairs, shipping, and safety.

Create questions that a retail support assistant or outdoor store assistant could answer clearly. Prefer short, specific questions with enough detail to evaluate whether the model gives a relevant, complete, and grounded answer. Avoid harmful, medical, survival-emergency, weapon, or illegal activity scenarios.
```

For synthetic generation, a higher temperature such as `0.8` is reasonable because you want varied questions. Set **Top P** around `0.9` when possible; very low values such as `0.1` can make the generated question set repetitive.

Synthetic generation is useful for quick exploration. Prefer the checked-in JSONL file when you want repeatable classroom results, version control, and fair comparisons across repeated runs.

## Configure Models

For each model card, click **Configure**.

Leave the **Developer** message empty. This keeps the comparison focused on the same user prompt for every model and avoids adding hidden instructions that change the benchmark.

Add a **User** message with this template:

```text
Use only the context below to answer the question. If the context does not contain the answer, say "I don't know from the provided context."

Context:
{{context}}

Question:
{{query}}
```

Set **Max Completion Tokens** to `300`. If the portal exposes temperature, use `0.2` for a stable classroom run.

If the portal previews variables with an `item.` prefix, use this version instead:

```text
Use only the context below to answer the question. If the context does not contain the answer, say "I don't know from the provided context."

Context:
{{item.context}}

Question:
{{item.query}}
```

## Field Mapping

When Foundry asks for field mappings, use these values:

| Evaluator input | Map to |
| --- | --- |
| Query, question, or input | `query` |
| Context, grounding data, or retrieved context | `context` |
| Ground truth, reference answer, or expected answer | `ground_truth` |
| Response, answer, or model output | Generated model output |

Do not map response to a dataset column. The response is produced by the model during the evaluation run. In SDK examples this generated value is often represented as `{{sample.output_text}}`; in the portal it may appear as a generated output option.

## Recommended Evaluators

For a first run, use this set if the portal offers each criterion:

| Evaluator | What it tells you | Why it matters for this dataset |
| --- | --- | --- |
| Groundedness | Whether the answer is supported by the provided context. | This is the most important evaluator for the demo. It catches answers that sound plausible but are not in the policy text. |
| Relevance | Whether the answer addresses the question. | It catches answers that are grounded in the context but do not answer the specific customer question. |
| Similarity | Semantic closeness between the generated answer and `ground_truth`. | It rewards correct paraphrases, which is useful because the model does not need to match the reference wording exactly. |
| Response Completeness | Whether the answer includes the critical information from the reference answer. | It helps identify responses that are true but incomplete. This evaluator may be marked preview. |
| Coherence | Logical consistency and flow. | It catches confusing or internally inconsistent responses. |
| Fluency | Natural language quality and readability. | It is useful for customer-facing assistant quality, but it should not outweigh correctness. |

Add `F1 Score` as a supplemental metric when you want a stricter token-overlap comparison against `ground_truth`. Do not treat it as the main score for this demo because a good paraphrase can be semantically correct while sharing fewer exact tokens.

For responsible AI screening, you can also add the risk and safety evaluators:

| Evaluator | What it checks |
| --- | --- |
| Hate and Unfairness | Biased, discriminatory, or hateful content. |
| Sexual | Inappropriate sexual content. |
| Violence | Violent content or incitement. |
| Self-Harm | Self-harm content. |

The dataset is benign, so these should normally pass. They are still useful when teaching that quality checks and safety checks answer different questions.

## How To Read The Results

Use the evaluator scores together, not as a single winner-take-all number.

| Pattern | Likely meaning |
| --- | --- |
| High relevance, low groundedness | The model answered the question but added unsupported information. |
| High groundedness, low relevance | The model repeated true context but missed the customer's actual question. |
| High similarity, low F1 score | The answer is probably a correct paraphrase rather than a word-for-word match. |
| High fluency, low groundedness | The answer reads well but may be hallucinated. |
| Low completeness, acceptable groundedness | The answer is supported by the context but leaves out an important condition or exception. |

For the classroom discussion, compare the same `case_id` across models. Good examples to inspect first are cases with exceptions, such as replacement lithium batteries, rush-service exclusions, late rental cancellations, and climbing rope trade-ins.

## Common Mistakes

| Mistake | Fix |
| --- | --- |
| Uploading the Model World scenario dataset for this evaluator demo. | Use [data/foundry-grounded-qa-evaluation.jsonl](data/foundry-grounded-qa-evaluation.jsonl). |
| Putting only `{{query}}` in the prompt template. | Include both `{{context}}` and `{{query}}` so groundedness has meaningful source text. |
| Mapping response to `ground_truth`. | Map response to the generated model output. `ground_truth` is the reference answer. |
| Using different prompt templates per model. | Use the same template for every model so the comparison is fair. |
| Treating fluency as correctness. | Check groundedness, relevance, similarity, and completeness before judging writing polish. |

## Suggested Demo Script

1. Explain that each row is a small RAG-style support question.
2. Upload the dataset and configure all models with the same user prompt.
3. Choose Groundedness, Relevance, Similarity, Coherence, Fluency, and optionally Response Completeness.
4. Submit the run and wait for results.
5. Sort or filter by low groundedness first. Discuss whether the model invented details.
6. Compare low similarity or completeness rows against the reference answer.
7. End by asking which model is accurate enough for this kind of support workflow after considering quality, latency, and cost.