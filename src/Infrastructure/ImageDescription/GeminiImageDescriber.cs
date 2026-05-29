using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.ImageDescription;

public class GeminiImageDescriber(HttpClient httpClient, IHttpClientFactory httpClientFactory) : ImageDescriberHandler
{
    private const string Model = "gemini-2.5-flash";

    public override async Task<string> DescribeAsync(string imageUrl, string context, CancellationToken ct = default)
    {
        try
        {
            var imageClient = httpClientFactory.CreateClient("ImageDownloader");
            var imageBytes = await imageClient.GetByteArrayAsync(imageUrl, ct);
            var base64 = Convert.ToBase64String(imageBytes);
            var mimeType = InferMimeType(imageUrl);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"models/{Model}:generateContent");
            request.Content = JsonContent.Create(new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { inline_data = new { mime_type = mimeType, data = base64 } },
                            new { text = BuildPrompt(context) }
                        }
                    }
                }
            });

            var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Gemini {(int)response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(ct)
                ?? throw new HttpRequestException("Gemini returned empty response.");

            return result.Candidates[0].Content.Parts[0].Text;
        }
        catch
        {
            return await TryNextAsync(imageUrl, context, ct);
        }
    }

    private static string BuildPrompt(string context) =>
        $"以下是這張圖片在筆記中的上下文：\n{context}\n\n請詳細描述這張圖片的內容，聚焦在與上下文相關的資訊。";

    private static string InferMimeType(string url)
    {
        var lower = url.ToLowerInvariant();
        if (lower.Contains(".png")) return "image/png";
        if (lower.Contains(".gif")) return "image/gif";
        if (lower.Contains(".webp")) return "image/webp";
        return "image/jpeg";
    }
}

file record GeminiResponse(
    [property: JsonPropertyName("candidates")] List<GeminiCandidate> Candidates);

file record GeminiCandidate(
    [property: JsonPropertyName("content")] GeminiContent Content,
    [property: JsonPropertyName("finishReason")] string? FinishReason);

file record GeminiContent(
    [property: JsonPropertyName("parts")] List<GeminiPart> Parts,
    [property: JsonPropertyName("role")] string? Role);

file record GeminiPart(
    [property: JsonPropertyName("text")] string Text);
