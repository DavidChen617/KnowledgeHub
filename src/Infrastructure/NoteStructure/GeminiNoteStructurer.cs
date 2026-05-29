using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.AI;
using Domain.Exceptions;

namespace Infrastructure.NoteStructure;

public class GeminiNoteStructurer(HttpClient httpClient, IHttpClientFactory httpClientFactory) : INoteStructurer
{
    private const string Model = "gemini-2.5-flash";
    private const int MaxToolRounds = 10;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string SystemPrompt = """
        你是一個筆記整理助手。根據使用者提供的筆記內容與指示，以 JSON 格式回傳以下兩個欄位：
        - description: 一句話摘要，說明筆記的核心內容
        - structured_content: 結構化的 Markdown 內容，每個段落以 ### 開頭

        筆記中可能包含圖片，當圖片內容對理解筆記有幫助時，使用 read_image 工具讀取。非必要不讀取圖片。
        只回傳 JSON，不要有其他說明文字。
        """;

    public async Task<NoteStructureResult> StructureAsync(string content, string userPrompt, CancellationToken ct = default)
    {
        var contents = new List<GeminiContent>
        {
            new("user", [new GeminiPart { Text = $"{userPrompt}\n\n{content}" }])
        };

        for (var round = 0; round < MaxToolRounds; round++)
        {
            var geminiResponse = await callGeminiAsync(contents, ct);
            var parts = geminiResponse.Candidates[0].Content.Parts;

            var functionCall = parts.FirstOrDefault(p => p.FunctionCall is not null)?.FunctionCall;
            if (functionCall is null)
            {
                var text = parts.FirstOrDefault(p => p.Text is not null)?.Text
                    ?? throw new AiServiceException("Gemini 回傳空的 response。");

                var dto = JsonSerializer.Deserialize<GeminiNoteStructureDto>(ExtractJson(text), JsonOptions)
                    ?? throw new AiServiceException("Gemini 回傳的 JSON 無法解析。");

                return new NoteStructureResult(dto.Description, dto.StructuredContent);
            }

            contents.Add(geminiResponse.Candidates[0].Content);
            contents.Add(await buildToolResponseAsync(functionCall, ct));
        }

        throw new AiServiceException("Gemini tool call 超過上限。");

        // ── local functions ──────────────────────────────────────────

        async Task<GeminiResponse> callGeminiAsync(IReadOnlyList<GeminiContent> c, CancellationToken token)
        {
            var body = new GeminiRequest(
                new GeminiSystemInstruction([new GeminiPart { Text = SystemPrompt }]),
                c,
                [new GeminiTool([
                    new GeminiFunctionDeclaration(
                        "read_image",
                        "讀取筆記中的圖片，當文字描述不足以理解內容時使用",
                        new GeminiFunctionParameters(
                            "object",
                            new Dictionary<string, GeminiProperty> { ["url"] = new("string", "圖片的 URL") },
                            ["url"]
                        )
                    )
                ])]
            );

            using var req = new HttpRequestMessage(HttpMethod.Post, $"models/{Model}:generateContent");
            req.Content = JsonContent.Create(body, options: JsonOptions);

            HttpResponseMessage resp;
            try
            {
                resp = await httpClient.SendAsync(req, token);
            }
            catch (Exception e)
            {
                throw new AiServiceException(e.Message);
            }

            if ((int)resp.StatusCode is >= 400 and <= 499)
                throw new AiRateLimitException($"Gemini rate limit: {await resp.Content.ReadAsStringAsync(token)}");

            if (!resp.IsSuccessStatusCode)
                throw new AiServiceException($"Gemini error {(int)resp.StatusCode}: {await resp.Content.ReadAsStringAsync(token)}");

            return await resp.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, token)
                ?? throw new AiServiceException("Gemini 回傳空的 response。");
        }

        async Task<GeminiContent> buildToolResponseAsync(GeminiFunctionCall call, CancellationToken token)
        {
            if (call.Name != "read_image")
                throw new AiServiceException($"未知的 tool: {call.Name}");

            var url = call.Args.GetProperty("url").GetString()
                ?? throw new AiServiceException("read_image 缺少 url 參數");

            var imageClient = httpClientFactory.CreateClient("ImageDownloader");
            byte[] bytes;
            string mimeType;
            try
            {
                var imageResp = await imageClient.GetAsync(url, token);
                mimeType = imageResp.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                bytes = await imageResp.Content.ReadAsByteArrayAsync(token);
            }
            catch (Exception e)
            {
                throw new AiServiceException($"圖片下載失敗: {e.Message}");
            }

            return new GeminiContent("user",
            [
                new GeminiPart
                {
                    FunctionResponse = new GeminiFunctionResponse(call.Name, new { status = "ok" })
                },
                new GeminiPart
                {
                    InlineData = new GeminiInlineData(mimeType, Convert.ToBase64String(bytes))
                }
            ]);
        }
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }
}

// ── Request ──────────────────────────────────────────────────────────

file record GeminiRequest(
    [property: JsonPropertyName("system_instruction")] GeminiSystemInstruction SystemInstruction,
    [property: JsonPropertyName("contents")] IReadOnlyList<GeminiContent> Contents,
    [property: JsonPropertyName("tools")] GeminiTool[] Tools
);

file record GeminiSystemInstruction(
    [property: JsonPropertyName("parts")] GeminiPart[] Parts
);

file record GeminiContent(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("parts")] GeminiPart[] Parts
);

file record GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("inline_data")]
    public GeminiInlineData? InlineData { get; init; }

    [JsonPropertyName("functionCall")]
    public GeminiFunctionCall? FunctionCall { get; init; }

    [JsonPropertyName("functionResponse")]
    public GeminiFunctionResponse? FunctionResponse { get; init; }
}

file record GeminiInlineData(
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("data")] string Data
);

file record GeminiFunctionCall(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("args")] JsonElement Args
);

file record GeminiFunctionResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("response")] object Response
);

file record GeminiTool(
    [property: JsonPropertyName("function_declarations")] GeminiFunctionDeclaration[] FunctionDeclarations
);

file record GeminiFunctionDeclaration(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("parameters")] GeminiFunctionParameters Parameters
);

file record GeminiFunctionParameters(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("properties")] Dictionary<string, GeminiProperty> Properties,
    [property: JsonPropertyName("required")] string[] Required
);

file record GeminiProperty(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("description")] string Description
);

// ── Response ─────────────────────────────────────────────────────────

file record GeminiResponse(
    [property: JsonPropertyName("candidates")] GeminiCandidate[] Candidates
);

file record GeminiCandidate(
    [property: JsonPropertyName("content")] GeminiContent Content
);

file record GeminiNoteStructureDto
{
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("structured_content")]
    public required string StructuredContent { get; init; }
}
