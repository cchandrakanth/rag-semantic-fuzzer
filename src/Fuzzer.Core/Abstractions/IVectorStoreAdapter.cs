using Fuzzer.Core.Models;

namespace Fuzzer.Core.Abstractions;

/// <summary>
/// Adapter interface for vector store queries.
/// </summary>
public interface IVectorStoreAdapter
{
    Task<IReadOnlyList<VectorChunk>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int topK = 3, float similarityThreshold = 0.7f, CancellationToken cancellationToken = default);
}
