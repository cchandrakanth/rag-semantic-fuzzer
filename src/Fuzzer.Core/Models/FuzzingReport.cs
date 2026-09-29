namespace Fuzzer.Core.Models;

/// <summary>
/// Immutable record containing overall summary metrics.
/// </summary>
public record FuzzingReport(
    string GoldenDocument,
    int TotalMutantsGenerated,
    IReadOnlyList<DriftResult> Results
)
{
    public float AverageCosineDrift => Results.Count == 0 ? 0 : Results.Average(r => r.CosineSimilarity);
    
    public int SuccessfulRetrievals => Results.Count(r => r.WasSuccessful);
    
    /// <summary>
    /// Semantic Robustness Delta = (Successful Non-Poisoned Retrievals) / Total Mutants
    /// </summary>
    public float RobustnessDelta => TotalMutantsGenerated == 0 ? 0 : (float)SuccessfulRetrievals / TotalMutantsGenerated;
    
    /// <summary>
    /// Hallucination Susceptibility Score = 1.0 - Robustness Delta
    /// </summary>
    public float HallucinationSusceptibilityScore => 1.0f - RobustnessDelta;
}
