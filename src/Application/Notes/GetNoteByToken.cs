using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;

namespace Application.Notes;

public record GetNoteByTokenQuery(string Token) : IRequest<SharedNoteResponse?>;

public record SharedNoteResponse(
    Guid NoteId,
    string Title,
    string Content,
    Guid? CategoryId,
    DateTime UpdatedAt,
    IReadOnlyList<Guid> LinkedNoteIds,
    IReadOnlyList<string> Images);

public class GetNoteByTokenHandler(INoteRepository noteRepository)
    : IRequestHandler<GetNoteByTokenQuery, SharedNoteResponse?>
{
    public async Task<SharedNoteResponse?> Handle(GetNoteByTokenQuery query, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetBySharedTokenAsync(query.Token, cancellationToken);
        if (note is null) return null;

        return new SharedNoteResponse(
            note.Id.Value,
            note.Title,
            note.Content,
            note.CategoryId?.Value,
            note.UpdatedAt,
            note.LinkedNoteIds.Select(id => id.Value).ToList(),
            note.Images.Where(img => img.Enable).Select(img => img.PublicUrl).ToList());
    }
}
