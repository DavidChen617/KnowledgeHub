using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ShareKernal;

namespace Infrastructure.Embedding;

public class MistralEmbedder(HttpClient httpClient) : EmbedderHandler
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
            model = "mistral-embed",
            input = texts
        });

        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Mistral {(int)response.StatusCode}");

        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(ct)
            ?? throw new HttpRequestException("Mistral returned empty response.");

        return result.Data.Select(d => d.Embedding).ToArray();
    }
}

file record EmbedResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("data")] List<EmbedData> Data,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("usage")] EmbedUsage? Usage);

file record EmbedData(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("embedding")] float[] Embedding);

file record EmbedUsage(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens,
    [property: JsonPropertyName("prompt_audio_seconds")] double? PromptAudioSeconds,
    [property: JsonPropertyName("request_count")] int? RequestCount);
