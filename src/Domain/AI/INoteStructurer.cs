using ShareKernal;

namespace Domain.AI;

public interface INoteStructurer
{
    Task<Result<NoteStructureResult>> StructureAsync(string content, string userPrompt, CancellationToken cancellationToken = default);
}
