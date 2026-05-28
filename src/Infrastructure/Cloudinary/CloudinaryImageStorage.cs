using Domain.Notes;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cloudinary;

internal sealed class CloudinaryImageStorage(ILogger<CloudinaryImageStorage> logger) : IImageStorage
{
    public Task DeleteAsync(string publicUrl, CancellationToken ct = default)
    {
        logger.LogInformation("TODO: delete Cloudinary image {Url}", publicUrl);
        return Task.CompletedTask;
    }

    public Task DeleteManyAsync(IEnumerable<string> publicUrls, CancellationToken ct = default)
    {
        foreach (var url in publicUrls)
            logger.LogInformation("TODO: delete Cloudinary image {Url}", url);
        return Task.CompletedTask;
    }
}
