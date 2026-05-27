namespace Domain.AI;

public interface INoteStructurer
{
    Task<NoteStructureResult> StructureAsync(string content, string userPrompt, CancellationToken cancellationToken = default);
}
