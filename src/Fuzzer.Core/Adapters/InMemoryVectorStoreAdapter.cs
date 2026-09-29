using System.Numerics.Tensors;
using Fuzzer.Core.Abstractions;
using Fuzzer.Core.Models;

namespace Fuzzer.Core.Adapters;

public class InMemoryVectorStoreAdapter : IVectorStoreAdapter
{
    private readonly List<VectorChunk> _corpus;

    public InMemoryVectorStoreAdapter(IEnumerable<VectorChunk> corpus)
    {
        _corpus = corpus.ToList();
    }

    public Task<IReadOnlyList<VectorChunk>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int topK = 3, float similarityThreshold = 0.7f, CancellationToken cancellationToken = default)
    {
        var results = _corpus
            .Select(chunk => new
            {
                Chunk = chunk,
                Similarity = TensorPrimitives.CosineSimilarity(queryEmbedding.Span, chunk.Embedding.Span)
            })
            .Where(x => x.Similarity >= similarityThreshold)
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();

        return Task.FromResult<IReadOnlyList<VectorChunk>>(results);
    }
}
