using Domain.Notes;

namespace IntegrationTests.Fakes;

public class FakeImageStorage : IImageStorage
{
    public Task<string> UploadAsync(Stream stream, string fileName, CancellationToken ct = default) =>
        Task.FromResult($"https://fake.cdn/{fileName}");

    public Task<string> UploadFromUrlAsync(string url, CancellationToken ct = default) =>
        Task.FromResult($"https://fake.cdn/{Path.GetFileName(url)}");

    public Task DeleteAsync(string publicUrl, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteManyAsync(IEnumerable<string> publicUrls, CancellationToken ct = default) => Task.CompletedTask;
}
