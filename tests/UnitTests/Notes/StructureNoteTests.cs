using System.Net.Http.Headers;
using Domain.AI;
using Domain.Notes;
using Domain.Users;
using Infrastructure.Embedding;
using Infrastructure.NoteStructure;
using Microsoft.Extensions.Configuration;

namespace UnitTests.Notes;

file class LoggingHandler : DelegatingHandler
{
    public LoggingHandler() : base(new HttpClientHandler()) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Console.WriteLine($"[read_image] {request.RequestUri}");
        return await base.SendAsync(request, ct);
    }
}

file class SimpleHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(new LoggingHandler());
}

[Trait("Category", "Integration")]
public class StructureNoteTests
{
    private static readonly IConfiguration Config = new ConfigurationBuilder()
        .AddUserSecrets<StructureNoteTests>()
        .Build();

    private static HttpClient GroqClient()
    {
        var client = new HttpClient { BaseAddress = new Uri("https://api.groq.com/openai/v1/") };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Config["Groq:ApiKey"]);
        return client;
    }

    private static HttpClient GeminiClient()
    {
        var client = new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/") };
        client.DefaultRequestHeaders.Add("x-goog-api-key", Config["Gemini:ApiKey"]);
        return client;
    }

    private static HttpClient CohereClient()
    {
        var client = new HttpClient { BaseAddress = new Uri("https://api.cohere.com/v2/") };
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Config["Cohere:ApiKey"]);
        return client;
    }

    private static IReadOnlyList<(int, string)> HeadingMapper(string content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        var chunks = new List<(int, string)>();
        var currentLines = new List<string>();
        var index = 0;

        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith("### ") && currentLines.Count > 0)
            {
                chunks.Add((index++, string.Join('\n', currentLines).Trim()));
                currentLines.Clear();
            }
            currentLines.Add(line);
        }

        if (currentLines.Count > 0)
        {
            var text = string.Join('\n', currentLines).Trim();
            if (!string.IsNullOrEmpty(text))
                chunks.Add((index, text));
        }

        return chunks;
    }

    [Fact]
    public async Task FullFlow_NoteLifecycle()
    {
        var refId1 = Guid.NewGuid();
        var refId2 = Guid.NewGuid();
        var refId3 = Guid.NewGuid();

        // --- 1. 建立筆記，內含連結 ---
        var initialContent = $"""
            今天學了 dependency injection，可以對照 [[{refId1}]] 的 SOLID 原則筆記。
            基本概念：不在 class 裡自己 new 物件，改從外部注入。
            .NET 有內建 DI container，在 Program.cs 用 builder.Services 註冊。
            有三種 lifetime：Singleton、Scoped、Transient。
            DbContext 要用 Scoped，HttpClient 要透過 IHttpClientFactory。
            相關範例可以看 [[{refId2}]] 的 repository pattern 筆記。
            """;

        var noteResult = Note.Create(UserId.New(), "Dependency Injection 筆記", initialContent);
        Assert.True(noteResult.IsSuccess);
        var note = noteResult.Value;

        // NoteParser 自動解析 [[uuid]] 連結
        Assert.Equal(2, note.LinkedNoteIds.Count);
        Assert.Contains(new NoteId(refId1), note.LinkedNoteIds);
        Assert.Contains(new NoteId(refId2), note.LinkedNoteIds);

        Console.WriteLine($"[1] 初始連結數：{note.LinkedNoteIds.Count}（{refId1}, {refId2}）");

        // --- 2. 更新內容，NoteLinkDiffer 計算差異 ---
        var updatedContent = $"""
            今天學了 dependency injection，可以對照 [[{refId2}]] 的 SOLID 原則筆記。
            基本概念：不在 class 裡自己 new 物件，改從外部注入。
            .NET 有內建 DI container，在 Program.cs 用 builder.Services 註冊。
            有三種 lifetime：Singleton、Scoped、Transient。
            DbContext 要用 Scoped，HttpClient 要透過 IHttpClientFactory。
            新增內容：可以搭配 [[{refId3}]] 的 unit test 筆記一起看。
            """;

        note.UpdateContent(updatedContent);

        // refId1 移除、refId3 新增
        Assert.Equal(2, note.LinkedNoteIds.Count);
        Assert.DoesNotContain(new NoteId(refId1), note.LinkedNoteIds);
        Assert.Contains(new NoteId(refId2), note.LinkedNoteIds);
        Assert.Contains(new NoteId(refId3), note.LinkedNoteIds);

        Console.WriteLine($"[2] 更新後連結：{string.Join(", ", note.LinkedNoteIds.Select(x => x.Value.ToString()[..8]))}");

        // --- 3. AI 結構化 ---
        var structurer = new GroqNoteStructurer(GroqClient());
        var structured = await structurer.StructureAsync(note.Content, "請將這篇筆記結構化，整理成清楚的重點");

        Assert.NotEmpty(structured.StructuredContent);
        Assert.Contains("###", structured.StructuredContent);

        Console.WriteLine($"\n[3] Groq 結構化結果：\n{structured.StructuredContent}");
        Console.WriteLine($"    描述：{structured.Description}");

        // --- 4. Chunk ---
        var rawChunks = Chunker.Chunk(structured.StructuredContent, HeadingMapper);
        var structure = note.AddStructure("請將這篇筆記結構化，整理成清楚的重點", structured.StructuredContent, structured.Description, rawChunks);

        Assert.Single(note.Structures);
        Assert.True(structure.Chunks.Count > 0);

        Console.WriteLine($"\n[4] Chunk 數量：{structure.Chunks.Count}");

        // --- 5. Embedding ---
        var embedder = new CohereEmbedder(CohereClient());

        foreach (var chunk in structure.Chunks)
        {
            var vector = await embedder.EmbedAsync(chunk.Artifact);
            chunk.SetEmbedding(vector);
        }

        Assert.All(structure.Chunks, c => Assert.NotNull(c.Embedding));
        Assert.All(structure.Chunks, c => Assert.Equal(c.Id, c.Embedding!.ChunkId));
        Assert.All(structure.Chunks, c => Assert.Equal(1536, c.Embedding!.Vector.Length));

        Console.WriteLine($"\n[5] Embedding 結果：");
        foreach (var chunk in structure.Chunks)
            Console.WriteLine($"    Chunk[{chunk.Index}] 維度={chunk.Embedding!.Vector.Length}, 前3值=[{string.Join(", ", chunk.Embedding.Vector[..3].Select(v => v.ToString("F4")))}]");
    }

    [Fact]
    public async Task Gemini_StructureNote_WithImages()
    {
        const string content = """
            # 學習筆記：MVC 架構設計模式

            今天在看 ASP.NET Core 的架構，順便複習了一下 MVC 的概念，把理解整理起來。

            MVC 把應用程式分成三個部分，彼此職責分離，下面這張圖說明了三者的互動關係：

            ![MVC 架構圖](https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/MVC-Process.svg/600px-MVC-Process.svg.png)

            看完圖之後大概懂了，使用者的操作先進 Controller，Controller 去問 Model 拿資料，再把結果丟給 View 顯示。

            之前一直搞混 Model 跟 ViewModel 的差別，現在知道 ViewModel 是專門給 View 用的資料結構，不是直接把 domain model 丟過去。

            ASP.NET Core 的 Controller 還有 ApiController 跟一般 Controller 的區別，ApiController 會自動處理 model binding 和 validation，不用自己寫。

            還要研究的：
            - Razor Pages 跟 MVC 的選擇時機
            - minimal API 跟 Controller-based API 的差異
            - 如何搭配 CQRS 讓 Controller 更薄
            """;

        var structurer = new GeminiNoteStructurer(GeminiClient(), new SimpleHttpClientFactory());
        var result = await structurer.StructureAsync(content, "請將這篇學習筆記結構化，整理成有條理的重點，圖片中有架構圖請仔細理解並納入分析");

        Assert.NotEmpty(result.StructuredContent);
        Assert.Contains("###", result.StructuredContent);
        Assert.NotEmpty(result.Description);

        Console.WriteLine($"描述：{result.Description}");
        Console.WriteLine($"\n結構化內容：\n{result.StructuredContent}");
    }
}
