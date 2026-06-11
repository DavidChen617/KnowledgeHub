using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using ShareKernal;

namespace Application.Images;

public record UploadImagesCommand(IReadOnlyList<ImageUploadItem> Items)
    : IRequest<Result<IReadOnlyList<UploadedImageResult>>>;

public record ImageUploadItem(Stream Content, string FileName);

public record UploadedImageResult(string OriginalName, string StorageKey);

public class UploadImagesHandler(IImageStorage imageStorage)
    : IRequestHandler<UploadImagesCommand, Result<IReadOnlyList<UploadedImageResult>>>
{
    public async Task<Result<IReadOnlyList<UploadedImageResult>>> Handle(
        UploadImagesCommand command, CancellationToken ct)
    {
        var tasks = command.Items.Select(async item =>
        {
            var key = await imageStorage.UploadAsync(item.Content, item.FileName, ct);
            return new UploadedImageResult(item.FileName, key);
        });

        return Result.Success<IReadOnlyList<UploadedImageResult>>(await Task.WhenAll(tasks));
    }
}
