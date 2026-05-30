using ShareKernal;

namespace Domain.AI;

public interface IImageDescriber
{
    Task<Result<string>> DescribeAsync(string imageUrl, string context, CancellationToken ct = default);
}
