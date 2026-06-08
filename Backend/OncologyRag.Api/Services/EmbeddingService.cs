using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OncologyRag.Api.Services;

public class EmbeddingService(IHttpClientFactory httpClientFactory, ILogger<EmbeddingService> logger)
{
    private const string VoyageModel = "voyage-3";
    private const string VoyageEndpoint = "https://api.voyageai.com/v1/embeddings";

    public async Task<float[]> EmbedAsync(string text, string apiKey, CancellationToken ct = default)
    {
        // Anthropic's embedding support is via Voyage AI — uses same API key
        var client = httpClientFactory.CreateClient("voyage");

        var payload = new { input = new[] { text }, model = VoyageModel };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, VoyageEndpoint)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Voyage embedding failed: {Status} {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Embedding request failed: {response.StatusCode}");
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(body);
        var embedding = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();

        return embedding;
    }
}
