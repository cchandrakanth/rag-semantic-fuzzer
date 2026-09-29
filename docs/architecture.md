# Architecture & Methodology

## Overview
`rag-semantic-fuzzer` evaluates the semantic robustness of RAG (Retrieval-Augmented Generation) pipelines through targeted textual mutations. Traditional unit testing often fails to capture "semantic drift"—where subtly altered queries result in substantially different (and often incorrect or poisoned) retrieved context.

## Core Components
1. **Mutation Engine**: Generates semantic perturbations.
2. **Text Embedding Service**: Generates embeddings for original and mutated queries.
3. **Vector Store Adapter**: Executes vector similarity searches against a simulated or real document corpus.
4. **Drift Calculator (TensorPrimitives)**: Utilizes `System.Numerics.Tensors.TensorPrimitives` for high-performance CPU-bound cosine similarity calculation.

## Metric Definitions
- **Semantic Robustness Delta**: The percentage of mutations that still successfully retrieve the correct context without capturing irrelevant or "poisoned" data.
- **Hallucination Susceptibility Score**: `1.0 - Robustness Delta`. Higher scores indicate the RAG system is highly prone to retrieving incorrect context when users slightly modify their phrasing.
