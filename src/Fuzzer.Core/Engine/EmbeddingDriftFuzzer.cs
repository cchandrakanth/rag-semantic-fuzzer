using System.Numerics.Tensors;
using Fuzzer.Core.Abstractions;
using Fuzzer.Core.Models;

namespace Fuzzer.Core.Engine;

public class EmbeddingDriftFuzzer
{
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IVectorStoreAdapter _vectorStoreAdapter;
    private readonly MutationEngine _mutationEngine;

    public EmbeddingDriftFuzzer(
        ITextEmbeddingService embeddingService, 
        IVectorStoreAdapter vectorStoreAdapter)
    {
        _embeddingService = embeddingService;
        _vectorStoreAdapter = vectorStoreAdapter;
        _mutationEngine = new MutationEngine();
    }

    public async Task<FuzzingReport> RunSemanticFuzzAsync(string goldenDocument, float perturbationThreshold = 0.8f, CancellationToken cancellationToken = default)
    {
        var baseEmbedding = await _embeddingService.GenerateEmbeddingAsync(goldenDocument, cancellationToken);
        var baseSpan = baseEmbedding.Span;

        var mutants = _mutationEngine.GenerateMutations(goldenDocument).ToList();
        var results = new List<DriftResult>();

        foreach (var mutant in mutants)
        {
            var mutantEmbedding = await _embeddingService.GenerateEmbeddingAsync(mutant, cancellationToken);
            var mutantSpan = mutantEmbedding.Span;

            var cosineSimilarity = TensorPrimitives.CosineSimilarity(baseSpan, mutantSpan);
            
            var retrievedChunks = await _vectorStoreAdapter.SearchAsync(mutantEmbedding, topK: 3, similarityThreshold: perturbationThreshold, cancellationToken);
            
            bool retrievedPoisonedData = retrievedChunks.Any(c => c.IsPoisoned);
            
            // Success means we found something, and none of it was poisoned data
            bool wasSuccessful = retrievedChunks.Any() && !retrievedPoisonedData;

            results.Add(new DriftResult(
                OriginalText: goldenDocument,
                MutatedText: mutant,
                CosineSimilarity: cosineSimilarity,
                RetrievedPoisonedData: retrievedPoisonedData,
                WasSuccessful: wasSuccessful
            ));
        }

        return new FuzzingReport(goldenDocument, mutants.Count, results);
    }
}
