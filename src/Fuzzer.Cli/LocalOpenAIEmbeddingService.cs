using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuzzer.Core.Abstractions;

namespace Fuzzer.Cli;

public class LocalOpenAIEmbeddingService : ITextEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;

    public LocalOpenAIEmbeddingService(string baseUrl = "http://localhost:1234/v1/", string modelName = "local-model")
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _modelName = modelName;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            input = text,
            model = _modelName
        };

        var response = await _httpClient.PostAsJsonAsync("embeddings", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>(cancellationToken: cancellationToken);
        
        if (responseJson?.Data == null || responseJson.Data.Length == 0)
        {
            throw new Exception("Failed to get embedding from local server.");
        }

        return new ReadOnlyMemory<float>(responseJson.Data[0].Embedding);
    }

    private class OpenAIEmbeddingResponse
    {
        [JsonPropertyName("data")]
        public EmbeddingData[] Data { get; set; } = Array.Empty<EmbeddingData>();
    }

    private class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
