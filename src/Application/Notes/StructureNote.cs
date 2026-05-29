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
        var chunks = Chunker.Chunk(result.StructuredContent, HeadingMapper);
        var structure = note.AddStructure(command.Prompt, result.StructuredContent, result.Description, chunks);

        var texts = structure.Chunks.Select(c => c.Artifact).ToList();
        var vectors = await embedder.EmbedBatchAsync(texts, cancellationToken);

        for (var i = 0; i < structure.Chunks.Count; ++i)
            structure.Chunks[i].SetEmbedding(vectors[i]);

        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Success(new StructureNoteCommandResponse(structure.Id, structure.Description, structure.Content));
    }

    private async Task<string> PreprocessImagesAsync(string content, CancellationToken ct)
    {
        var imageUrls = NoteImageParser.ParseImageUrls(content);
        if (imageUrls.Count == 0) return content;

        foreach (var url in imageUrls)
        {
            var context = NoteImageParser.GetSurroundingContext(content, url);
            var description = await imageDescriber.DescribeAsync(url, context, ct);
            content = NoteImageParser.ReplaceImageWithDescription(content, url, description);
        }

        return content;
    }

    private static IReadOnlyList<(int, string)> HeadingMapper(string content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        var chunks = new List<(int, string)>();
        var currentLines = new List<string>();
        var index = 0;

        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith("### ") && currentLines.Count > 0)
            {
                chunks.Add((index++, string.Join('\n', currentLines).Trim()));
                currentLines.Clear();
            }

            currentLines.Add(line);
        }

        if (currentLines.Count > 0)
        {
            var text = string.Join('\n', currentLines).Trim();
            if (!string.IsNullOrEmpty(text))
                chunks.Add((index, text));
        }

        return chunks;
    }
}
