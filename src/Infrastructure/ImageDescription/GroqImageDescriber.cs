using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.ImageDescription;

public class GroqImageDescriber(HttpClient httpClient, IHttpClientFactory httpClientFactory) : ImageDescriberHandler
{
    private const string Model = "meta-llama/llama-4-scout-17b-16e-instruct";

    public override async Task<string> DescribeAsync(string imageUrl, string context, CancellationToken ct = default)
    {
        try
        {
            var imageClient = httpClientFactory.CreateClient("ImageDownloader");
            var imageBytes = await imageClient.GetByteArrayAsync(imageUrl, ct);
            var dataUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";

            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            request.Content = JsonContent.Create(new
            {
                model = Model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = BuildPrompt(context) },
                            new { type = "image_url", image_url = new { url = dataUrl } }
                        }
                    }
                }
            });

            var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Groq {(int)response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<GroqResponse>(ct)
                ?? throw new HttpRequestException("Groq returned empty response.");

            return result.Choices[0].Message.Content;
        }
        catch
        {
            return await TryNextAsync(imageUrl, context, ct);
        }
    }

    private static string BuildPrompt(string context) =>
        $"以下是這張圖片在筆記中的上下文：\n{context}\n\n請詳細描述這張圖片的內容，聚焦在與上下文相關的資訊。";
}

file record GroqResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("object")] string? Object,
    [property: JsonPropertyName("created")] long? Created,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("choices")] List<GroqChoice> Choices,
    [property: JsonPropertyName("usage")] GroqUsage? Usage);

file record GroqChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("finish_reason")] string? FinishReason,
    [property: JsonPropertyName("message")] GroqMessage Message);

file record GroqMessage(
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("content")] string Content);

file record GroqUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int? TotalTokens,
    [property: JsonPropertyName("prompt_time")] double? PromptTime,
    [property: JsonPropertyName("completion_time")] double? CompletionTime,
    [property: JsonPropertyName("total_time")] double? TotalTime);
