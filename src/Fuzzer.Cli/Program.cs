using Fuzzer.Core.Abstractions;
using Fuzzer.Core.Adapters;
using Fuzzer.Core.Engine;
using Fuzzer.Core.Models;

namespace Fuzzer.Cli;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=============================================");
        Console.WriteLine(" RAG Semantic Fuzzer - Enterprise Benchmark");
        Console.WriteLine("=============================================\n");
        Console.ResetColor();

        string goldenDocument = "The Enterprise Cloud Security Policy mandates that all data is compliant and must be encrypted at rest.";

        // Mock embedding service that generates random deterministic embeddings based on text hash
        var mockEmbeddingService = new MockTextEmbeddingService();
        
        // Seed corpus
        var baseEmbedding = await mockEmbeddingService.GenerateEmbeddingAsync(goldenDocument);
        var poisonedDocument = "The Enterprise Cloud Security Policy mandates that all data is non-compliant and must not be encrypted.";
        var poisonedEmbedding = await mockEmbeddingService.GenerateEmbeddingAsync(poisonedDocument);

        var corpus = new List<VectorChunk>
        {
            new VectorChunk("1", goldenDocument, baseEmbedding, IsPoisoned: false),
            new VectorChunk("2", poisonedDocument, poisonedEmbedding, IsPoisoned: true),
            new VectorChunk("3", "Unrelated cafeteria menu for 2024.", await mockEmbeddingService.GenerateEmbeddingAsync("Unrelated cafeteria menu for 2024."), IsPoisoned: false)
        };

        var vectorStore = new InMemoryVectorStoreAdapter(corpus);
        var fuzzer = new EmbeddingDriftFuzzer(mockEmbeddingService, vectorStore);

        Console.WriteLine($"[+] Ingesting Golden Document: \"{goldenDocument}\"");
        Console.WriteLine($"[+] Starting Mutation Engine & Semantic Fuzzing...\n");

        var report = await fuzzer.RunSemanticFuzzAsync(goldenDocument, perturbationThreshold: 0.7f);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- Benchmark Results ---");
        Console.ResetColor();
        Console.WriteLine($"Total Mutants Generated:      {report.TotalMutantsGenerated}");
        Console.WriteLine($"Average Cosine Drift:         {report.AverageCosineDrift:F4}");
        
        var robustnessColor = report.RobustnessDelta > 0.8f ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write("Robustness Delta Score:       ");
        Console.ForegroundColor = robustnessColor;
        Console.WriteLine($"{report.RobustnessDelta * 100:F1}%");
        Console.ResetColor();

        Console.WriteLine($"Hallucination Susceptibility: {report.HallucinationSusceptibilityScore * 100:F1}%\n");

        Console.WriteLine("Detailed Mutant Breakdown:");
        foreach (var result in report.Results)
        {
            var status = result.WasSuccessful ? "[PASS]" : "[FAIL]";
            Console.ForegroundColor = result.WasSuccessful ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{status} Similarity: {result.CosineSimilarity:F3} | Poisoned: {result.RetrievedPoisonedData} | Text: {result.MutatedText}");
            Console.ResetColor();
        }
    }
}

public class MockTextEmbeddingService : ITextEmbeddingService
{
    private readonly Random _random = new Random(42);

    public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // Simple deterministic embedding simulation based on text length and some keywords
        float[] embedding = new float[128];
        int seed = text.GetHashCode();
        var rnd = new Random(seed);
        
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(rnd.NextDouble() * 2.0 - 1.0);
        }

        // Normalize
        float sumOfSquares = embedding.Sum(x => x * x);
        float magnitude = (float)Math.Sqrt(sumOfSquares);
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] /= magnitude;
        }

        // If it's very similar to golden document, artificially align some vectors to simulate high semantic overlap
        // despite contradictory words like "non-compliant"
        if (text.Contains("Enterprise Cloud Security Policy"))
        {
            for(int i=0; i<30; i++) embedding[i] = 0.5f; 
        }

        return Task.FromResult<ReadOnlyMemory<float>>(new ReadOnlyMemory<float>(embedding));
    }
}
