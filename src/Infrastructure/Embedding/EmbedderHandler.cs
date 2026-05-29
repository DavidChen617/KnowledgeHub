using Domain.AI;
using Domain.Exceptions;

namespace Infrastructure.Embedding;

public abstract class EmbedderHandler : IEmbedder
{
    private EmbedderHandler? _next;

    public EmbedderHandler SetNext(EmbedderHandler next)
    {
        _next = next;
        return next;
    }

    public abstract Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
    public abstract Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);

    protected Task<float[]> TryNextAsync(string text, CancellationToken ct)
    {
        if (_next is null)
            throw new AiServiceException("All embedders in the chain exhausted.");
        return _next.EmbedAsync(text, ct);
    }

    protected Task<float[][]> TryNextBatchAsync(IReadOnlyList<string> texts, CancellationToken ct)
    {
        if (_next is null)
            throw new AiServiceException("All embedders in the chain exhausted.");
        return _next.EmbedBatchAsync(texts, ct);
    }
}
