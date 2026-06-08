using ShareKernal;

namespace Domain.AI;

public interface IEmbedder
{
    Task<Result<float[]>> EmbedAsync(string text, CancellationToken cancellationToken = default);
    Task<Result<float[][]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);
}
