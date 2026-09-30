using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuzzer.Core.Engine;

public class MutationEngine
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;

    public MutationEngine(string baseUrl = "http://localhost:1234/v1/", string modelName = "google/gemma-4-26b-a4b-qat")
    {
        _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromHours(1) // Gemma needs more than 100 seconds for massive reasoning blocks!
        };
        _modelName = modelName;
    }

    /// <summary>
    /// Generates mutated variants of the provided text using a local LLM.
    /// </summary>
    public async Task<IEnumerable<string>> GenerateMutationsAsync(string goldenDocument, int targetCount = 1000, CancellationToken cancellationToken = default)
    {
        var mutations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int batchSize = 200;
        
        Console.WriteLine($"\n[+] Starting LLM Batch Generation. Target: {targetCount} unique mutations.");
        
        while (mutations.Count < targetCount && !cancellationToken.IsCancellationRequested)
        {
            var systemPrompt = $"You are an adversarial AI. Generate {batchSize} distinct, contradictory mutations of the provided document. The mutations must retain similar vocabulary but invert or drastically change the logical meaning. Output ONLY the {batchSize} sentences, one per line. Do not include numbers, markdown, or any introductory text. Do not repeat sentences.";
            
            var requestBody = new
            {
                model = _modelName,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = goldenDocument }
                },
                temperature = 0.9, // Higher temp for more variety
                max_tokens = 120000
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody, cancellationToken);
                response.EnsureSuccessStatusCode();

                var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var responseJson = JsonSerializer.Deserialize<OpenAIChatResponse>(rawJson);
                var content = responseJson?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

                var batchMutations = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(m => m.Trim().TrimStart("1234567890.- *".ToCharArray()))
                                       .Where(m => !string.IsNullOrWhiteSpace(m) && m != goldenDocument);
                
                int previousCount = mutations.Count;
                foreach (var m in batchMutations)
                {
                    mutations.Add(m);
                    if (mutations.Count >= targetCount) break;
                }
                
                Console.WriteLine($"[DEBUG] Generated {mutations.Count - previousCount} unique mutants in this batch. (Total: {mutations.Count}/{targetCount})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Batch failed: {ex.Message}. Retrying...");
                await Task.Delay(1000, cancellationToken);
            }
        }

        return mutations;
    }

    private class OpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; } = new Message();
    }

    private class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
