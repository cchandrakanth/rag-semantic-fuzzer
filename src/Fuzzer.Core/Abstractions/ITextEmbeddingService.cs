namespace Fuzzer.Core.Abstractions;

/// <summary>
/// Service contract for generating embeddings.
/// </summary>
public interface ITextEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
