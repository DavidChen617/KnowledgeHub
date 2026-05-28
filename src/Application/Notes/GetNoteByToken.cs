using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;

namespace Application.Notes;

public record GetNoteByTokenQueryRequest(string Token) : IRequest<GetNoteByTokenQueryResponse?>;

public record GetNoteByTokenQueryResponse(
    Guid NoteId,
    string Title,
    string Content,
    Guid? CategoryId,
    DateTime UpdatedAt,
    IReadOnlyList<Guid> LinkedNoteIds,
    IReadOnlyList<string> Images,
    SharePermission Permission);

public class GetNoteByTokenHandler(INoteRepository noteRepository)
    : IRequestHandler<GetNoteByTokenQueryRequest, GetNoteByTokenQueryResponse?>
{
    public async Task<GetNoteByTokenQueryResponse?> Handle(GetNoteByTokenQueryRequest query, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetBySharedTokenAsync(query.Token, cancellationToken);
        if (note is null) return null;

        return new GetNoteByTokenQueryResponse(
            note.Id.Value,
            note.Title,
            note.Content,
            note.CategoryId?.Value,
            note.UpdatedAt,
            note.LinkedNoteIds.Select(id => id.Value).ToList(),
            note.Images.Where(img => img.Enable).Select(img => img.PublicUrl).ToList(),
            note.SharedLink!.Permission);
    }
}
