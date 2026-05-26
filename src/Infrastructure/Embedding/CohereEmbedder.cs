using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.AI;
using Domain.Exceptions;

namespace Infrastructure.Embedding;

public class CohereEmbedder(HttpClient httpClient) : IEmbedder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var result = await EmbedBatchAsync([text], cancellationToken);
        return result[0];
    }

    public async Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "embed");
        request.Content = JsonContent.Create(new
        {
            model = "embed-v4.0",
            input_type = "search_document",
            embedding_types = new[] { "float" },
            texts
        });

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception e)
        {
            throw new AiServiceException(e.Message);
        }

        if ((int)response.StatusCode is >= 400 and <= 499)
            throw new AiRateLimitException($"Cohere rate limit: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        if (!response.IsSuccessStatusCode)
            throw new AiServiceException($"Cohere error {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(JsonOptions, cancellationToken)
            ?? throw new AiServiceException("Cohere 回傳空的 response。");

        return result.Embeddings.Float;
    }
}

file record EmbedResponse
{
    public required Embeddings Embeddings { get; init; }
}

file record Embeddings
{
    [JsonPropertyName("float")]
    public required float[][] Float { get; init; }
}
