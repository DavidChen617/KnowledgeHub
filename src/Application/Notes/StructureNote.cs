using Application.Interfaces;
using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.NoteStructure;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record StructureNoteCommandRequest(NoteId NoteId, string Prompt, Guid UserId)
    : IRequest<Result<StructureNoteCommandResponse>>;

public record StructureNoteCommandResponse(Guid StructureId, string Description, string Content);

public class StructureNoteHandler(
    INoteRepository noteRepository,
    INoteStructurer structurer,
    IEmbedder embedder,
    IImageDescriber imageDescriber,
    IUnitOfWork unitOfWork,
    IStructureRateLimiter rateLimiter)
    : IRequestHandler<StructureNoteCommandRequest, Result<StructureNoteCommandResponse>>
{
    public async Task<Result<StructureNoteCommandResponse>> Handle(StructureNoteCommandRequest command,
        CancellationToken cancellationToken = default)
    {
        if (!await rateLimiter.IsAllowedAsync(command.UserId, cancellationToken))
            return new Error("RateLimit.Exceeded", "AI 結構化功能每小時限制 5 次，請稍後再試。", ErrorType.TooManyRequests);

        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);

        if (note is null)
            return NotFound;

        var preprocessResult = await PreprocessImagesAsync(note.Content, cancellationToken);
        if (!preprocessResult.IsSuccess) return preprocessResult.Error;

        var structureResult = await structurer.StructureAsync(preprocessResult.Value, command.Prompt, cancellationToken);
        if (!structureResult.IsSuccess) return structureResult.Error;

        var structure = note.AddStructure(command.Prompt, structureResult.Value.StructuredContent, structureResult.Value.Description);

        var texts = structure.GetChunks();
        var vectorsResult = await embedder.EmbedBatchAsync(texts, cancellationToken);
        if (!vectorsResult.IsSuccess) return vectorsResult.Error;

        structure.SetEmbedding(vectorsResult.Value);

        await noteRepository.Update(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new StructureNoteCommandResponse(structure.Id, structure.Description, structure.Content));
    }

    private async Task<Result<string>> PreprocessImagesAsync(NoteContent noteContent, CancellationToken ct)
    {
        if (noteContent.ImageUrls.Count == 0) return noteContent.Value;

        var text = noteContent.Value;
        foreach (var url in noteContent.ImageUrls)
        {
            var context = noteContent.GetSurroundingContext(url);
            var describeResult = await imageDescriber.DescribeAsync(url, context, ct);
            if (!describeResult.IsSuccess) return describeResult.Error;
            text = NoteContent.ReplaceImageWithDescription(text, url, describeResult.Value);
        }

        return text;
    }
}
