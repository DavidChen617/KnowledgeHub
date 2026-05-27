using CoreMesh.Dispatching.Abstractions;
using Domain.AI;
using Domain.Notes;
using Domain.Shared;

namespace Application.Notes;

public record StructureNoteCommandRequest(NoteId NoteId, string Prompt) : IRequest<StructureNoteCommandResponse>;

public record StructureNoteCommandResponse(Guid StructureId, string Description, string Content);

public class StructureNoteHandler(INoteRepository noteRepository, INoteStructurer structurer, IEmbedder embedder, IUnitOfWork unitOfWork)
    : IRequestHandler<StructureNoteCommandRequest, StructureNoteCommandResponse?>
{
    public async Task<StructureNoteCommandResponse?> Handle(StructureNoteCommandRequest command, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        
        if(note is null)
            return null;
        
        var result = await structurer.StructureAsync(note.Content, command.Prompt, cancellationToken);
        var chunks = Chunker.Chunk(result.StructuredContent, HeadingMapper);
        var structure = note.AddStructure(command.Prompt, result.StructuredContent, result.Description, chunks);

        var texts = structure.Chunks.Select(c => c.Artifact).ToList();
        var vectors = await embedder.EmbedBatchAsync(texts, cancellationToken);

        for (var i = 0; i < structure.Chunks.Count; ++i)
            structure.Chunks[i].SetEmbedding(vectors[i]);

        await noteRepository.UpdateAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new StructureNoteCommandResponse(structure.Id, structure.Description, structure.Content);
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
