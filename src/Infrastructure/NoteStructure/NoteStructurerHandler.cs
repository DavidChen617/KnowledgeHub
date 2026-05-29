using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.AI;
using Domain.Exceptions;

namespace Infrastructure.NoteStructure;

public abstract class NoteStructurerHandler : INoteStructurer
{
    private NoteStructurerHandler? _next;

    protected abstract string Model { get; }
    protected abstract HttpClient HttpClient { get; }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string SystemPrompt = """
        你是一個筆記整理助手。根據使用者提供的筆記內容與指示，以 JSON 格式回傳以下兩個欄位：
        - description: 一句話摘要，說明筆記的核心內容
        - structured_content: 結構化的 Markdown 內容，每個段落以 ### 開頭

        只回傳 JSON，不要有其他說明文字。
        """;

    public NoteStructurerHandler SetNext(NoteStructurerHandler next)
    {
        _next = next;
        return next;
    }

    public async Task<NoteStructureResult> StructureAsync(string content, string userPrompt, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            request.Content = JsonContent.Create(new
            {
                model = Model,
                temperature = 0.2,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user",   content = $"{userPrompt}\n\n{content}" }
                }
            });

            var response = await HttpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"{GetType().Name} {(int)response.StatusCode}");

            var chatResponse = await response.Content.ReadFromJsonAsync<StructurerChatResponse>(JsonOptions, ct)
                ?? throw new HttpRequestException($"{GetType().Name} returned empty response.");

            var json = chatResponse.Choices[0].Message.Content;
            var dto = JsonSerializer.Deserialize<NoteStructureDto>(ExtractJson(json), JsonOptions)
                ?? throw new HttpRequestException($"{GetType().Name} returned unparseable JSON.");

            return new NoteStructureResult(dto.Description, dto.StructuredContent);
        }
        catch
        {
            return await TryNextAsync(content, userPrompt, ct);
        }
    }

    protected Task<NoteStructureResult> TryNextAsync(string content, string userPrompt, CancellationToken ct)
    {
        if (_next is null) throw new AiServiceException("All note structurers in the chain exhausted.");
        return _next.StructureAsync(content, userPrompt, ct);
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }
}

file record NoteStructureDto
{
    public required string Description { get; init; }

    [JsonPropertyName("structured_content")]
    public required string StructuredContent { get; init; }
}

file record StructurerChatResponse(
    [property: JsonPropertyName("choices")] List<StructurerChatChoice> Choices);

file record StructurerChatChoice(
    [property: JsonPropertyName("message")] StructurerChatMessage Message);

file record StructurerChatMessage(
    [property: JsonPropertyName("content")] string Content);
