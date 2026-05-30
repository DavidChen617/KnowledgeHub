using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ShareKernal;

namespace Infrastructure.Embedding;

public class OpenRouterEmbedder(HttpClient httpClient) : EmbedderHandler
{
    public override async Task<Result<float[]>> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var result = await CallApiAsync([text], ct);
            return Result.Success(result[0]);
        }
        catch
        {
            return await TryNextAsync(text, ct);
        }
    }

    public override async Task<Result<float[][]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        try
        {
            return Result.Success(await CallApiAsync(texts, ct));
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
            model = "baai/bge-m3",
            input = texts
        });

        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"OpenRouter {(int)response.StatusCode}");

        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(ct)
            ?? throw new HttpRequestException("OpenRouter returned empty response.");

        return result.Data.Select(d => d.Embedding).ToArray();
    }
}

file record EmbedResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("data")] List<EmbedData> Data,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("provider")] string? Provider,
    [property: JsonPropertyName("usage")] EmbedUsage? Usage);

file record EmbedData(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("embedding")] float[] Embedding);

file record EmbedUsage(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens,
    [property: JsonPropertyName("cost")] double? Cost);
