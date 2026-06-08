using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ShareKernal;

namespace Infrastructure.ImageDescription;

public class OpenRouterImageDescriber(HttpClient httpClient) : ImageDescriberHandler
{
    private const string Model = "openrouter/auto";

    public override async Task<Result<string>> DescribeAsync(string imageUrl, string context, CancellationToken ct = default)
    {
        try
        {
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
                            new { type = "image_url", image_url = new { url = imageUrl } }
                        }
                    }
                }
            });

            var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"OpenRouter {(int)response.StatusCode}");

            var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(ct)
                ?? throw new HttpRequestException("OpenRouter returned empty response.");

            return Result.Success(result.Choices[0].Message.Content);
        }
        catch
        {
            return await TryNextAsync(imageUrl, context, ct);
        }
    }

    private static string BuildPrompt(string context) =>
        $"以下是這張圖片在筆記中的上下文：\n{context}\n\n請詳細描述這張圖片的內容，聚焦在與上下文相關的資訊。";
}

file record OpenRouterResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("choices")] List<OpenRouterChoice> Choices,
    [property: JsonPropertyName("usage")] OpenRouterUsage? Usage);

file record OpenRouterChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("finish_reason")] string? FinishReason,
    [property: JsonPropertyName("message")] OpenRouterMessage Message);

file record OpenRouterMessage(
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("content")] string Content);

file record OpenRouterUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int? TotalTokens,
    [property: JsonPropertyName("cost")] double? Cost);
