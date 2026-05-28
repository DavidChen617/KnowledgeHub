using Domain.Users;

namespace Domain.Notes;

public interface INoteRepository
{
    Task AddAsync(Note note, CancellationToken ct = default);
    Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default);
    Task<Note?> GetByIdAndUserIdAsync(NoteId id, UserId userId, CancellationToken ct = default);
    Task<IReadOnlyList<Note>> GetAllByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task<IReadOnlyList<Note>> SearchByTitleAsync(UserId userId, string title, CancellationToken ct = default);
    Task<Note?> GetBySharedTokenAsync(string token, CancellationToken ct = default);
    Task UpdateAsync(Note note, CancellationToken ct = default);
    Task DeleteAsync(Note note, CancellationToken ct = default);
}
