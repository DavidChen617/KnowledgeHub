using ShareKernal;

namespace Domain.NoteStructure;

public interface IImageDescriber
{
    Task<Result<string>> DescribeAsync(string imageUrl, string context, CancellationToken ct = default);
}
