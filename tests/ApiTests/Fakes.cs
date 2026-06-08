using System.Text.Json;
using Application.Interfaces;
using CoreMesh.Outbox.Abstractions;
using Domain.AI;
using Domain.Notes;
using Domain.Notifications;
using ShareKernal;

namespace ApiTests;

public class FakeNoteStructurer : INoteStructurer
{
    public Task<Result<NoteStructureResult>> StructureAsync(string content, string userPrompt, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new NoteStructureResult("Fake structure", "### 標題\n內容")));
}

public class FakeEmbedder : IEmbedder
{
    private static readonly float[] Vector = Enumerable.Repeat(0.1f, 1024).ToArray();
    public Task<Result<float[]>> EmbedAsync(string text, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(Vector));
    public Task<Result<float[][]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(texts.Select(_ => Vector).ToArray()));
}

public class FakeImageDescriber : IImageDescriber
{
    public Task<Result<string>> DescribeAsync(string imageUrl, string context, CancellationToken ct = default) =>
        Task.FromResult(Result.Success("fake description"));
}

public class FakeEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default) =>
        Task.CompletedTask;
}

public class FakeImageStorage : IImageStorage
{
    public Task<string> UploadAsync(Stream stream, string fileName, CancellationToken ct = default) =>
        Task.FromResult($"https://fake.cdn/{fileName}");
    public Task<string> UploadFromUrlAsync(string url, CancellationToken ct = default) =>
        Task.FromResult($"https://fake.cdn/{Path.GetFileName(url)}");
    public Task DeleteAsync(string publicUrl, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteManyAsync(IEnumerable<string> publicUrls, CancellationToken ct = default) => Task.CompletedTask;
}

public class FakeEventPublisher : IEventPublisher
{
    public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class FakeMessageSubscriber : IMessageSubscriber
{
    public async IAsyncEnumerable<EventEnvelope> SubscribeAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }

    public Task AckAsync(EventEnvelope envelope, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task NackAsync(EventEnvelope envelope, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task RetryAsync(EventEnvelope envelope, CancellationToken cancellationToken) => Task.CompletedTask;
}

public class FakeCacher : ICacher
{
    private readonly Dictionary<string, string> _store = new();
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_store.TryGetValue(key, out var json))
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        return Task.FromResult<T?>(default);
    }
    public Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct = default)
    {
        _store[key] = JsonSerializer.Serialize(value);
        return Task.CompletedTask;
    }
    public Task RemoveAsync(string key, CancellationToken ct = default) { _store.Remove(key); return Task.CompletedTask; }
    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        foreach (var k in _store.Keys.Where(k => k.StartsWith(prefix)).ToList()) _store.Remove(k);
        return Task.CompletedTask;
    }
}
