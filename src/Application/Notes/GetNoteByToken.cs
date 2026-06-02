using Application.Interfaces;
using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

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
    string AuthorName,
    string AuthorEmail,
    string? AuthorAvatarUrl);

public class GetNoteByTokenHandler(INoteRepository noteRepository, IUserRepository userRepository, ICacher cacher)
    : IRequestHandler<GetNoteByTokenQueryRequest, Result<GetNoteByTokenQueryResponse>>
{
    public async Task<Result<GetNoteByTokenQueryResponse>> Handle(GetNoteByTokenQueryRequest query, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.NoteByToken(query.Token);

        var cached = await cacher.GetAsync<GetNoteByTokenQueryResponse>(key, cancellationToken);
        if (cached is not null) return Success(cached);

        var note = await noteRepository.GetBySharedTokenAsync(query.Token, cancellationToken);
        if (note is null) return TokenNotFound;

        var author = await userRepository.GetByIdAsync(note.UserId, cancellationToken);

        var response = new GetNoteByTokenQueryResponse(
            note.Id.Value,
            note.Title,
            note.Content,
            note.CategoryId?.Value,
            note.UpdatedAt,
            note.LinkedNoteIds.Select(id => id.Value).ToList(),
            note.Images.Where(img => img.Enable).Select(img => img.PublicUrl).ToList(),
            author?.Username ?? string.Empty,
            author?.Email ?? string.Empty,
            author?.AvatarUrl);

        await cacher.SetAsync(key, response, TimeSpan.FromMinutes(5), cancellationToken);

        return Success(response);
    }
}
