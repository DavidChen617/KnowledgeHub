using System.Net.Http.Headers;
using Domain.AI;
using Domain.Notes;
using Domain.Notes.Events;
using Domain.Users;
using Infrastructure.Embedding;
using Infrastructure.NoteStructure;
using Microsoft.Extensions.Configuration;

namespace UnitTests.Notes;

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

        var note = Note.Create(UserId.New(), "Dependency Injection 筆記", initialContent);

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

        var linkEvent = note.DomainEvents.OfType<NoteLinksChangedEvent>().Last();
        Assert.Contains(new NoteId(refId3), linkEvent.ToAdd);
        Assert.Contains(new NoteId(refId1), linkEvent.ToRemove);

        Console.WriteLine($"[2] 更新後連結：{string.Join(", ", note.LinkedNoteIds.Select(x => x.Value.ToString()[..8]))}");
        Console.WriteLine($"    新增：{string.Join(", ", linkEvent.ToAdd.Select(x => x.Value.ToString()[..8]))}");
        Console.WriteLine($"    移除：{string.Join(", ", linkEvent.ToRemove.Select(x => x.Value.ToString()[..8]))}");

        // --- 3. AI 結構化 ---
        var structurer = new GroqNoteStructurer(GroqClient());
        var structured = await structurer.StructureAsync(note.Content, "請將這篇筆記結構化，整理成清楚的重點");

        Assert.NotEmpty(structured);
        Assert.Contains("###", structured);

        Console.WriteLine($"\n[3] Groq 結構化結果：\n{structured}");

        // --- 4. Chunk ---
        var rawChunks = Chunker.Chunk(structured, HeadingMapper);
        var structure = note.AddStructure("請將這篇筆記結構化，整理成清楚的重點", structured, rawChunks);

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
}
