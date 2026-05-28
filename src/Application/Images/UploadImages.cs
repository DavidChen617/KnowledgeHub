using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;

namespace Application.Images;

public record UploadImagesCommand(IReadOnlyList<ImageUploadItem> Items)
    : IRequest<IReadOnlyList<UploadedImageResult>>;

public record ImageUploadItem(Stream Content, string FileName);

public record UploadedImageResult(string OriginalName, string StorageKey);

public class UploadImagesHandler(IImageStorage imageStorage)
    : IRequestHandler<UploadImagesCommand, IReadOnlyList<UploadedImageResult>>
{
    public async Task<IReadOnlyList<UploadedImageResult>> Handle(
        UploadImagesCommand command, CancellationToken ct)
    {
        var tasks = command.Items.Select(async item =>
        {
            var key = await imageStorage.UploadAsync(item.Content, item.FileName, ct);
            return new UploadedImageResult(item.FileName, key);
        });

        return await Task.WhenAll(tasks);
    }
}
