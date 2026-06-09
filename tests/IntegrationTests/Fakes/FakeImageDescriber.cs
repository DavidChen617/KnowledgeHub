using Domain.NoteStructure;
using ShareKernal;

namespace IntegrationTests.Fakes;

public class FakeImageDescriber : IImageDescriber
{
    public Task<Result<string>> DescribeAsync(string imageUrl, string context, CancellationToken ct = default) =>
        Task.FromResult(Result.Success("fake description"));
}
