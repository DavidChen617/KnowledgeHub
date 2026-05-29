using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Embedding;

public class GeminiEmbedder(HttpClient httpClient) : EmbedderHandler
{
    private const string Model = "gemini-embedding-001";
    private const int Dimension = 1024;

    public override async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"models/{Model}:embedContent");
            request.Content = JsonContent.Create(new
            {
                model = $"models/{Model}",
                content = new { parts = new[] { new { text } } },
                outputDimensionality = Dimension
            });

            var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini {(int)response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(ct)
                ?? throw new HttpRequestException("Gemini returned empty response.");

            return result.Embedding.Values;
        }
        catch
        {
            return await TryNextAsync(text, ct);
        }
    }

    public override async Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"models/{Model}:batchEmbedContents");
            request.Content = JsonContent.Create(new
            {
                requests = texts.Select(t => new
                {
                    model = $"models/{Model}",
                    content = new { parts = new[] { new { text = t } } },
                    outputDimensionality = Dimension
                }).ToArray()
            });

            var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini {(int)response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<BatchEmbedResponse>(ct)
                ?? throw new HttpRequestException("Gemini returned empty response.");

            return result.Embeddings.Select(e => e.Values).ToArray();
        }
        catch
        {
            return await TryNextBatchAsync(texts, ct);
        }
    }
}

file record EmbedResponse(
    [property: JsonPropertyName("embedding")] EmbedPayload Embedding,
    [property: JsonPropertyName("usageMetadata")] EmbedUsageMetadata? UsageMetadata);

file record EmbedPayload(
    [property: JsonPropertyName("values")] float[] Values);

file record EmbedUsageMetadata(
    [property: JsonPropertyName("promptTokenCount")] int PromptTokenCount,
    [property: JsonPropertyName("promptTokenDetails")] List<PromptTokenDetail>? PromptTokenDetails);

file record PromptTokenDetail(
    [property: JsonPropertyName("modality")] string Modality,
    [property: JsonPropertyName("tokenCount")] int TokenCount);

file record BatchEmbedResponse(
    [property: JsonPropertyName("embeddings")] List<EmbedPayload> Embeddings);
