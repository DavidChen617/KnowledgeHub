using Domain.NoteStructure;
using ShareKernal;

namespace Infrastructure.ImageDescription;

public abstract class ImageDescriberHandler : IImageDescriber
{
    private ImageDescriberHandler? _next;

    public ImageDescriberHandler SetNext(ImageDescriberHandler next)
    {
        _next = next;
        return next;
    }

    public abstract Task<Result<string>> DescribeAsync(string imageUrl, string context, CancellationToken ct = default);

    protected Task<Result<string>> TryNextAsync(string imageUrl, string context, CancellationToken ct)
    {
        if (_next is null) return Task.FromResult(Result.Failure<string>(AiErrors.ChainExhausted));
        return _next.DescribeAsync(imageUrl, context, ct);
    }
}
