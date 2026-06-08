using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Notes;

public record GetNoteQueryRequest(NoteId NoteId, UserId UserId) : IRequest<Result<GetNoteQueryResponse>>;

public record GetNoteQueryResponse(
    Guid NoteId,
    string Title,
    string Content,
    Guid? CategoryId,
    IReadOnlyList<Guid> LinkedNoteIds,
    string? SharedToken,
    DateTime UpdatedAt);

public class GetNoteHandler(INoteRepository noteRepository)
    : IRequestHandler<GetNoteQueryRequest, Result<GetNoteQueryResponse>>
{
    public async Task<Result<GetNoteQueryResponse>> Handle(GetNoteQueryRequest query, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAndUserIdAsync(query.NoteId, query.UserId, cancellationToken);

        if (note is null)
            return NotFound;

        return Success(new GetNoteQueryResponse(
            note.Id.Value,
            note.Title,
            note.Content,
            note.CategoryId?.Value,
            note.LinkedNoteIds.Select(id => id.Value).ToList(),
            note.SharedLinkToken,
            note.UpdatedAt));
    }
}
