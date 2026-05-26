using System.Net.Http.Json;
using System.Text.Json;
using Domain.AI;
using Domain.Exceptions;

namespace Infrastructure.NoteStructure;

public class GroqNoteStructurer(HttpClient httpClient) : INoteStructurer
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string SystemPrompt =
        "你是一個筆記助手，請根據使用者的指示將筆記內容結構化，使用 Markdown 格式，每個段落以 ### 開頭。只回傳結構化後的內容，不要有多餘說明。";

    public async Task<string> StructureAsync(string content, string prompt, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Content = JsonContent.Create(new
        {
            model = "llama-3.3-70b-versatile",
            temperature = 0.2,
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = $"{prompt}\n\n{content}" }
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

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(JsonOptions, cancellationToken)
            ?? throw new AiServiceException("Groq 回傳空的 response。");

        return result.Choices[0].Message.Content;
    }
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
