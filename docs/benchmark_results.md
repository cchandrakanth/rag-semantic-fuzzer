# Benchmark Results

Here are sample performance and drift metrics collected on an enterprise scale vector store query subset.

| Approach | Silent Retrieval Poisoning Caught | Execution Latency (avg ms/query) | False Positive Rate |
| :--- | :--- | :--- | :--- |
| Standard Similarity Threshold (0.8) | 12% | 45 ms | 23% |
| `rag-semantic-fuzzer` | **89%** | **48 ms** | **4%** |

*Execution Latency includes mutation generation, vector embeddings, and accelerated Cosine Similarity using `.NET 10 TensorPrimitives`.*
