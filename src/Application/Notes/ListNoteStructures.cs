using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record ListNoteStructuresQuery(NoteId NoteId, UserId UserId)
    : IRequest<Result<ListNoteStructuresDto>>;

public record NoteStructureSummary(Guid StructureId, string Description, string Content, DateTime CreatedAt);

public record ListNoteStructuresDto(IReadOnlyList<NoteStructureSummary> Structures);

public class ListNoteStructuresHandler(INoteRepository noteRepository)
    : IRequestHandler<ListNoteStructuresQuery, Result<ListNoteStructuresDto>>
{
    public async Task<Result<ListNoteStructuresDto>> Handle(
        ListNoteStructuresQuery query, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(query.NoteId, query.UserId, cancellationToken);

        if (note is null)
            return NotFound;

        var summaries = note.Structures
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new NoteStructureSummary(s.Id, s.Description, s.Content, s.CreatedAt))
            .ToList();

        return Success(new ListNoteStructuresDto(summaries));
    }
}
