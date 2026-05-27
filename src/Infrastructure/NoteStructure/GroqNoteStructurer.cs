using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.AI;
using Domain.Exceptions;

namespace Infrastructure.NoteStructure;

public class GroqNoteStructurer(HttpClient httpClient) : INoteStructurer
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string SystemPrompt = """
        你是一個筆記整理助手。根據使用者提供的筆記內容與指示，以 JSON 格式回傳以下兩個欄位：
        - description: 一句話摘要，說明筆記的核心內容
        - structured_content: 結構化的 Markdown 內容，每個段落以 ### 開頭

        只回傳 JSON，不要有其他說明文字。
        """;

    public async Task<NoteStructureResult> StructureAsync(string content, string userPrompt, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Content = JsonContent.Create(new
        {
            model = "llama-3.3-70b-versatile",
            temperature = 0.2,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = $"{userPrompt}\n\n{content}" }
            }
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
            throw new AiRateLimitException($"Groq rate limit: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        if (!response.IsSuccessStatusCode)
            throw new AiServiceException($"Groq error {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, cancellationToken)
            ?? throw new AiServiceException("Groq 回傳空的 response。");

        var json = chatResponse.Choices[0].Message.Content;

        var dto = JsonSerializer.Deserialize<NoteStructureDto>(json, JsonOptions)
            ?? throw new AiServiceException("Groq 回傳的 JSON 無法解析。");

        return new NoteStructureResult(dto.Description, dto.StructuredContent);
    }
}

file record NoteStructureDto
{
    public required string Description { get; init; }

    [JsonPropertyName("structured_content")]
    public required string StructuredContent { get; init; }
}

file record ChatResponse
{
    public required ChatChoice[] Choices { get; init; }
}

file record ChatChoice
{
    public required ChatMessage Message { get; init; }
}

file record ChatMessage
{
    public required string Content { get; init; }
}
