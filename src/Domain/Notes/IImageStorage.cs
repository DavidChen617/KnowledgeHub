namespace Domain.Notes;

public interface IImageStorage
{
    Task DeleteAsync(string publicUrl, CancellationToken ct = default);
    Task DeleteManyAsync(IEnumerable<string> publicUrls, CancellationToken ct = default);
}
