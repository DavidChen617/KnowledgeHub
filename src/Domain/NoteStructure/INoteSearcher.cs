using Domain.Notes;
using Domain.Users;
using ShareKernal;

namespace Domain.NoteStructure;

public interface INoteSearcher
{
    Task<Result<IReadOnlyList<NoteSearchResult>>> SearchAsync(UserId userId, string query, CancellationToken ct = default);
}

public record NoteSearchResult(NoteId NoteId, string Title, float Score);
