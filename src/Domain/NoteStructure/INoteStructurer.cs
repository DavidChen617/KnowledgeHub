using ShareKernal;

namespace Domain.NoteStructure;

public interface INoteStructurer
{
    Task<Result<NoteStructureResult>> StructureAsync(string content, string userPrompt, CancellationToken cancellationToken = default);
}
