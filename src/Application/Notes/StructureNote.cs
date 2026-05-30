using CoreMesh.Dispatching.Abstractions;
using Domain.AI;
using Domain.Notes;
using Domain.Shared;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record StructureNoteCommandRequest(NoteId NoteId, string Prompt)
    : IRequest<Result<StructureNoteCommandResponse>>;

public record StructureNoteCommandResponse(Guid StructureId, string Description, string Content);

public class StructureNoteHandler(
    INoteRepository noteRepository,
    INoteStructurer structurer,
    IEmbedder embedder,
    IImageDescriber imageDescriber,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StructureNoteCommandRequest, Result<StructureNoteCommandResponse>>
{
    public async Task<Result<StructureNoteCommandResponse>> Handle(StructureNoteCommandRequest command,
        CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);

        if (note is null)
            return NotFound;

        var content = await PreprocessImagesAsync(note.Content, cancellationToken);
        var result = await structurer.StructureAsync(content, command.Prompt, cancellationToken);
        var structure = note.AddStructure(command.Prompt, result.StructuredContent, result.Description);

        var texts = structure.Chunks.Select(c => c.Artifact).ToList();
        var vectors = await embedder.EmbedBatchAsync(texts, cancellationToken);

        for (var i = 0; i < structure.Chunks.Count; ++i)
            structure.Chunks[i].SetEmbedding(vectors[i]);

        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new StructureNoteCommandResponse(structure.Id, structure.Description, structure.Content));
    }

    private async Task<string> PreprocessImagesAsync(NoteContent noteContent, CancellationToken ct)
    {
        if (noteContent.ImageUrls.Count == 0) return noteContent.Value;

        var text = noteContent.Value;
        foreach (var url in noteContent.ImageUrls)
        {
            var context = noteContent.GetSurroundingContext(url);
            var description = await imageDescriber.DescribeAsync(url, context, ct);
            text = NoteContent.ReplaceImageWithDescription(text, url, description);
        }

        return text;
    }
}
