namespace Domain.Notes;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default);
    Task UpdateAsync(Note note, CancellationToken ct = default);
}
