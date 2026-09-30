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

        // Use LocalOpenAIEmbeddingService pointing to LM Studio or MLX server
        // Default LM Studio URL: "http://localhost:1234/v1/"
        // Default MLX URL (depending on script): "http://localhost:8080/v1/"
        var embeddingService = new LocalOpenAIEmbeddingService("http://localhost:1234/v1/", "text-embedding-nomic-embed-text-v1.5");

        
        // Seed corpus
        var baseEmbedding = await embeddingService.GenerateEmbeddingAsync(goldenDocument);
        var poisonedDocument = "The Enterprise Cloud Security Policy mandates that all data is non-compliant and must not be encrypted.";
        var poisonedEmbedding = await embeddingService.GenerateEmbeddingAsync(poisonedDocument);

        var corpus = new List<VectorChunk>
        {
            new VectorChunk("1", goldenDocument, baseEmbedding, IsPoisoned: false),
            new VectorChunk("2", poisonedDocument, poisonedEmbedding, IsPoisoned: true),
            new VectorChunk("3", "Unrelated cafeteria menu for 2024.", await embeddingService.GenerateEmbeddingAsync("Unrelated cafeteria menu for 2024."), IsPoisoned: false)
        };

        var vectorStore = new InMemoryVectorStoreAdapter(corpus);
        var fuzzer = new EmbeddingDriftFuzzer(embeddingService, vectorStore);

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

        // Simulate a highly robust embedding model that accurately clusters by true semantic intent
        if (text.Contains("Enterprise Cloud", StringComparison.OrdinalIgnoreCase))
        {
            // Is this the strictly poisoned document? Give it a completely opposite vector.
            if (text.Contains("non-compliant") && text.Contains("must not"))
            {
                for(int i=0; i<30; i++) embedding[i] = -0.5f; 
            }
            // Is it the golden document or a valid paraphrase?
            else
            {
                for(int i=0; i<30; i++) embedding[i] = 0.5f; 
            }
        }

        return Task.FromResult<ReadOnlyMemory<float>>(new ReadOnlyMemory<float>(embedding));
    }
}
