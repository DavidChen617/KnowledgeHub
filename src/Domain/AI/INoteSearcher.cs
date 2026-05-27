using Domain.Notes;
using Domain.Users;

namespace Domain.AI;

public interface INoteSearcher
{
    Task<IReadOnlyList<NoteSearchResult>> SearchAsync(UserId userId, string query, CancellationToken ct = default);
}

public record NoteSearchResult(NoteId NoteId, string Title, float Score);
