using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using ShareKernal;

namespace Application.Notes;

public record GetNoteByTokenQueryRequest(string Token) : IRequest<Result<GetNoteByTokenQueryResponse>>;

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
    : IRequestHandler<GetNoteByTokenQueryRequest, Result<GetNoteByTokenQueryResponse>>
{
    public async Task<Result<GetNoteByTokenQueryResponse>> Handle(GetNoteByTokenQueryRequest query, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetBySharedTokenAsync(query.Token, cancellationToken);
        if (note is null) return NoteErrors.TokenNotFound;

        return Result.Success(new GetNoteByTokenQueryResponse(
            note.Id.Value,
            note.Title,
            note.Content,
            note.CategoryId?.Value,
            note.UpdatedAt,
            note.LinkedNoteIds.Select(id => id.Value).ToList(),
            note.Images.Where(img => img.Enable).Select(img => img.PublicUrl).ToList(),
            note.SharedLink!.Permission));
    }
}
