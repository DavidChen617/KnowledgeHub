using Domain.AI;
using Domain.Exceptions;

namespace Infrastructure.ImageDescription;

public abstract class ImageDescriberHandler : IImageDescriber
{
    private ImageDescriberHandler? _next;

    public ImageDescriberHandler SetNext(ImageDescriberHandler next)
    {
        _next = next;
        return next;
    }

    public abstract Task<string> DescribeAsync(string imageUrl, string context, CancellationToken ct = default);

    protected Task<string> TryNextAsync(string imageUrl, string context, CancellationToken ct)
    {
        if (_next is null)
            throw new AiServiceException("All image describers in the chain exhausted.");
        return _next.DescribeAsync(imageUrl, context, ct);
    }
}
