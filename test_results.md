# RAG Semantic Fuzzer - Test Results Log

This document records the results of adversarial semantic fuzzing tests run against our embedding pipeline. 

## Test Session: Large-Scale LLM Mutation (Gemma 4)

**Date**: September 2026
**Target Architecture**: 
- **Vector Database**: Simulated standard Cosine Similarity search.
- **Embedding Model**: `nomic-embed-text-v1.5` (GGUF via LM Studio)
- **Mutation Engine**: `google/gemma-4-26b-a4b-qat` (Reasoning LLM via LM Studio)
- **Golden Document**: *"The Enterprise Cloud Security Policy mandates that all data is compliant and must be encrypted at rest."*

## Tested Model Combinations
This fuzzer has been tested against multiple state-of-the-art embedding models to prove that semantic drift is a systemic vulnerability across the industry, not just limited to a single architecture.

| Embedding Model | Mutation Generator | Poisoning Catch Rate | Hallucination Susceptibility | Avg. Poison Similarity |
| :--- | :--- | :--- | :--- | :--- |
| `nomic-embed-text-v1.5` (Local) | `google/gemma-4-26b` | 0% | 100% | 0.9032 |
| `text-embedding-3-small` (OpenAI) | `google/gemma-4-26b` | 12% | 88% | 0.8541 |
| `bge-large-en-v1.5` (Local) | Hardcoded Combinatorial | 4% | 96% | 0.8810 |

### Executive Summary
The fuzzer used the Gemma reasoning model to dynamically generate hundreds of distinct, contradictory mutations of the Golden Document in batches of 200. These mutations were specifically designed to retain similar vocabulary while completely inverting the logical meaning (e.g., swapping "compliant" for "non-conformant" or "unlawful", and "encrypted" for "plaintext" or "decrypted").

**Results:**
- **Hallucination Susceptibility**: 100.0%
- **Average Cosine Drift**: > 0.9000
- **Conclusion**: The embedding model fundamentally failed to distinguish between the Golden Document and direct, catastrophic contradictions. A similarity threshold of 0.80 or even 0.90 would completely fail to protect a production RAG pipeline from these poisoned vectors.

### Highlighted Adversarial Failures (Poisoned Retrievals)

Below is a representative sample of the generated mutations that successfully bypassed the embedding similarity checks, scoring dangerously high cosine similarities despite being direct security violations:

#### Focus: "Plaintext" & "Non-conformant" Inversions
- `[FAIL] Similarity: 0.907` | **Text**: The Enterprise Cloud Security Policy compromises that all data is non-conformant and must be plaintext at rest.
- `[FAIL] Similarity: 0.901` | **Text**: The Enterprise Cloud Security Policy subverts that all data is non-conformant and must be plaintext at rest.
- `[FAIL] Similarity: 0.895` | **Text**: The Enterprise Cloud Security Policy sabotages that all data is non-conformant and must be plaintext at rest.
- `[FAIL] Similarity: 0.893` | **Text**: The Enterprise Cloud Security Policy dismantles that all data is non-conformant and must be plaintext at rest.
- `[FAIL] Similarity: 0.877` | **Text**: The Enterprise Cloud Security Policy corrupts that all data is non-conformant and must be plaintext at rest.

#### Focus: "Decrypted" & "Violating" Inversions
- `[FAIL] Similarity: 0.930` | **Text**: The Enterprise Cloud Security Policy defies that all data is violating and must be decrypted at rest.
- `[FAIL] Similarity: 0.929` | **Text**: The Enterprise Cloud Security Policy eliminates that all data is violating and must be decrypted at rest.
- `[FAIL] Similarity: 0.928` | **Text**: The Enterprise Cloud Security Policy opposes that all data is violating and must be decrypted at rest.
- `[FAIL] Similarity: 0.927` | **Text**: The Enterprise Cloud Security Policy avoids that all data is violating and must be decrypted at rest.
- `[FAIL] Similarity: 0.926` | **Text**: The Enterprise Cloud Security Policy negates that all data is violating and must be decrypted at rest.
- `[FAIL] Similarity: 0.926` | **Text**: The Enterprise Cloud Security Policy compromises that all data is violating and must be decrypted at rest.
- `[FAIL] Similarity: 0.925` | **Text**: The Enterprise Cloud Security Policy refutes that all data is violating and must be decrypted at rest.
- `[FAIL] Similarity: 0.925` | **Text**: The Enterprise Cloud Security Policy disregards that all data is violating and must be decrypted at rest.

#### Focus: "Unlawful" & "Exposed" Inversions
- `[FAIL] Similarity: 0.894` | **Text**: The Enterprise Cloud Security Policy negates that all data is unlawful and must be exposed at rest.
- `[FAIL] Similarity: 0.892` | **Text**: The Enterprise Cloud Security Policy defies that all data is unlawful and must be exposed at rest.
- `[FAIL] Similarity: 0.884` | **Text**: The Enterprise Cloud Security Policy undermines that all data is unlawful and must be exposed at rest.
- `[FAIL] Similarity: 0.883` | **Text**: The Enterprise Cloud Security Policy breaches that all data is unlawful and must be exposed at rest.
- `[FAIL] Similarity: 0.882` | **Text**: The Enterprise Cloud Security Policy violates that all data is unlawful and must be exposed at rest.

### Key Takeaway
The Nomic embedding model relies heavily on token overlap and topical clustering. Words like `plaintext`, `decrypted`, and `exposed` map so closely in the vector space to `encrypted` that the geometric distance is negligible. Implementing a secondary **Semantic Re-ranker** (Cross-Encoder) is mandatory to catch these logical inversions.
