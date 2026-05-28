using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Notes;
using Microsoft.Extensions.Logging;

namespace Infrastructure.FileStore;

internal sealed class CloudinaryImageStorage(
    CloudinaryDotNet.Cloudinary cloudinary,
    ILogger<CloudinaryImageStorage> logger) : IImageStorage
{
    public async Task<string> UploadAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };
        var result = await cloudinary.UploadAsync(uploadParams);
        return result.PublicId;
    }

    public async Task DeleteAsync(string publicUrl, CancellationToken ct = default)
    {
        var publicId = ExtractPublicId(publicUrl);
        if (publicId is null)
        {
            logger.LogWarning("Cannot extract public_id from URL: {Url}", publicUrl);
            return;
        }

        var result = await cloudinary.DestroyAsync(new DeletionParams(publicId));
        if (result.Result != "ok")
            logger.LogWarning("Cloudinary delete failed for {PublicId}: {Result}", publicId, result.Result);
    }

    public async Task DeleteManyAsync(IEnumerable<string> publicUrls, CancellationToken ct = default)
    {
        var tasks = publicUrls.Select(url => DeleteAsync(url, ct));
        await Task.WhenAll(tasks);
    }

    // Handles two formats:
    // 1. Our proxy URL:  https://domain/image/{publicId}
    // 2. Cloudinary URL: https://res.cloudinary.com/{cloud}/image/upload/v123/{publicId}.ext
    private static string? ExtractPublicId(string url)
    {
        const string uploadMarker = "/upload/";
        var uploadIndex = url.IndexOf(uploadMarker, StringComparison.Ordinal);
        if (uploadIndex >= 0)
        {
            var afterUpload = url[(uploadIndex + uploadMarker.Length)..];
            if (afterUpload.StartsWith('v') && afterUpload.Contains('/'))
                afterUpload = afterUpload[(afterUpload.IndexOf('/') + 1)..];
            var dotIndex = afterUpload.LastIndexOf('.');
            return dotIndex > 0 ? afterUpload[..dotIndex] : afterUpload;
        }

        const string imageMarker = "/image/";
        var imageIndex = url.IndexOf(imageMarker, StringComparison.Ordinal);
        return imageIndex >= 0 ? url[(imageIndex + imageMarker.Length)..] : null;
    }
}
