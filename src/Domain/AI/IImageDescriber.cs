namespace Domain.AI;

public interface IImageDescriber
{
    Task<string> DescribeAsync(string imageUrl, string context, CancellationToken ct = default);
}
