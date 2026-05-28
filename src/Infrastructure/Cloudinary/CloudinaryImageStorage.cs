using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.Notes;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Cloudinary;

internal sealed class CloudinaryImageStorage(
    CloudinaryDotNet.Cloudinary cloudinary,
    ILogger<CloudinaryImageStorage> logger) : IImageStorage
{
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

    // https://res.cloudinary.com/{cloud}/image/upload/v123/{folder/public_id}.png
    // → folder/public_id
    private static string? ExtractPublicId(string url)
    {
        const string marker = "/upload/";
        var uploadIndex = url.IndexOf(marker, StringComparison.Ordinal);
        if (uploadIndex < 0) return null;

        var afterUpload = url[(uploadIndex + marker.Length)..];

        // 跳過版本號 v1234567/
        if (afterUpload.StartsWith('v') && afterUpload.Contains('/'))
            afterUpload = afterUpload[(afterUpload.IndexOf('/') + 1)..];

        // 移除副檔名
        var dotIndex = afterUpload.LastIndexOf('.');
        return dotIndex > 0 ? afterUpload[..dotIndex] : afterUpload;
    }
}
