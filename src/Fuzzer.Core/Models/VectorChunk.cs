namespace Fuzzer.Core.Models;

/// <summary>
/// Immutable record representing a chunk of data in the vector store.
/// </summary>
public record VectorChunk(
    string Id,
    string Text,
    ReadOnlyMemory<float> Embedding,
    bool IsPoisoned = false
);
