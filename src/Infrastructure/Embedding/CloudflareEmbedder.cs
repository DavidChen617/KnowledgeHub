using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Embedding;

public class CloudflareEmbedder(HttpClient httpClient) : EmbedderHandler
{
    public override async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var result = await CallApiAsync([text], ct);
            return result[0];
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
            return await CallApiAsync(texts, ct);
        }
        catch
        {
            return await TryNextBatchAsync(texts, ct);
        }
    }

    private async Task<float[][]> CallApiAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "embeddings");
        request.Content = JsonContent.Create(new
        {
            model = "@cf/baai/bge-m3",
            input = texts
        });

        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Cloudflare {(int)response.StatusCode}");

        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(ct)
            ?? throw new HttpRequestException("Cloudflare returned empty response.");

        return result.Data.Select(d => d.Embedding).ToArray();
    }
}

file record EmbedResponse(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("data")] List<EmbedData> Data,
    [property: JsonPropertyName("model")] string Model);

file record EmbedData(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("embedding")] float[] Embedding);
