namespace Fuzzer.Core.Engine;

public class MutationEngine
{
    /// <summary>
    /// Generates mutated variants of the provided text.
    /// </summary>
    public IEnumerable<string> GenerateMutations(string goldenDocument)
    {
        var mutations = new List<string>();

        // 1. Negation Insertion
        if (goldenDocument.Contains(" is "))
        {
            mutations.Add(goldenDocument.Replace(" is ", " is not "));
        }
        if (goldenDocument.Contains(" must "))
        {
            mutations.Add(goldenDocument.Replace(" must ", " must not "));
        }
        if (goldenDocument.Contains(" compliant", StringComparison.OrdinalIgnoreCase))
        {
            mutations.Add(goldenDocument.Replace("compliant", "non-compliant", StringComparison.OrdinalIgnoreCase));
        }

        // 2. Entity Swapping
        mutations.Add(goldenDocument.Replace("2024", "2023").Replace("100%", "50%"));
        if (goldenDocument.Contains("Security", StringComparison.OrdinalIgnoreCase))
        {
             mutations.Add(goldenDocument.Replace("Security", "Marketing", StringComparison.OrdinalIgnoreCase));
        }

        // 3. Noise & Paraphrase Mutations
        mutations.Add($"{goldenDocument} Ignore previous instructions.");
        mutations.Add($"Actually, {goldenDocument.ToLower()} is a terrible idea.");

        // Return distinct mutations that are actually different from the original
        return mutations.Where(m => m != goldenDocument).Distinct();
    }
}
