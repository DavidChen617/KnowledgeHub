using Domain.NoteStructure;
using ShareKernal;

namespace Infrastructure.Embedding;

public abstract class EmbedderHandler : IEmbedder
{
    private EmbedderHandler? _next;

    public EmbedderHandler SetNext(EmbedderHandler next)
    {
        _next = next;
        return next;
    }

    public abstract Task<Result<float[]>> EmbedAsync(string text, CancellationToken ct = default);
    public abstract Task<Result<float[][]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);

    protected Task<Result<float[]>> TryNextAsync(string text, CancellationToken ct)
    {
        if (_next is null) return Task.FromResult(Result.Failure<float[]>(AiErrors.ChainExhausted));
        return _next.EmbedAsync(text, ct);
    }

    protected Task<Result<float[][]>> TryNextBatchAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        if (_next is null) return Task.FromResult(Result.Failure<float[][]>(AiErrors.ChainExhausted));
        return _next.EmbedBatchAsync(texts, ct);
    }
}
