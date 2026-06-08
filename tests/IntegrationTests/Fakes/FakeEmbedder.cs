using Domain.AI;
using ShareKernal;

namespace IntegrationTests.Fakes;

public class FakeEmbedder : IEmbedder
{
    private static readonly float[] Vector = Enumerable.Repeat(0.1f, 1024).ToArray();

    public Task<Result<float[]>> EmbedAsync(string text, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(Vector));

    public Task<Result<float[][]>> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(texts.Select(_ => Vector).ToArray()));
}
