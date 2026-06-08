namespace Domain.Notes;

public interface IImageStorage
{
    Task<string> UploadAsync(Stream stream, string fileName, CancellationToken ct = default);
    Task<string> UploadFromUrlAsync(string url, CancellationToken ct = default);
    Task DeleteAsync(string publicUrl, CancellationToken ct = default);
    Task DeleteManyAsync(IEnumerable<string> publicUrls, CancellationToken ct = default);
}
