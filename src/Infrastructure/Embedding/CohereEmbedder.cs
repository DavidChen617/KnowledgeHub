using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ShareKernal;

namespace Infrastructure.Embedding;

public class CohereEmbedder(HttpClient httpClient) : EmbedderHandler
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "embed");
        request.Content = JsonContent.Create(new
        {
            model = "embed-v4.0",
            input_type = "search_document",
            embedding_types = new[] { "float" },
            output_dimension = 1024,
            texts
        });

        var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Cohere {(int)response.StatusCode}");

        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(ct)
            ?? throw new HttpRequestException("Cohere returned empty response.");

        return result.Embeddings.Float;
    }
}

file record EmbedResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("texts")] List<string> Texts,
    [property: JsonPropertyName("embeddings")] CohereEmbeddings Embeddings);

file record CohereEmbeddings(
    [property: JsonPropertyName("float")] float[][] Float);
