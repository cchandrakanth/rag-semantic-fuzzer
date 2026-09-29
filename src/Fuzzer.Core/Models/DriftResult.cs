namespace Fuzzer.Core.Models;

/// <summary>
/// Immutable record storing the result of a single mutation evaluation.
/// </summary>
public record DriftResult(
    string OriginalText,
    string MutatedText,
    float CosineSimilarity,
    bool RetrievedPoisonedData,
    bool WasSuccessful
);
