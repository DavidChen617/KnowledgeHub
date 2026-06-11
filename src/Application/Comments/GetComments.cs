using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Notes;
using Domain.Users;
using ShareKernal;
using static Application.Notes.NoteErrors;
using static ShareKernal.Result;

namespace Application.Comments;

public record GetCommentsQuery(NoteId NoteId, UserId? UserId, string? ShareToken) : IRequest<Result<GetCommentsDto>>;

public record CommentResponse(
    Guid CommentId,
    Guid? ParentCommentId,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Content,
    int LikeCount,
    bool LikedByMe,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetCommentsDto(IReadOnlyList<CommentResponse> Comments);

public class GetCommentsHandler(
    INoteRepository noteRepository,
    ICommentRepository commentRepository,
    IUserRepository userRepository)
    : IRequestHandler<GetCommentsQuery, Result<GetCommentsDto>>
{
    public async Task<Result<GetCommentsDto>> Handle(GetCommentsQuery query, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(query.NoteId, cancellationToken);
        if (note is null) return NotFound;

        var isOwner = query.UserId is not null && note.UserId == query.UserId;
        var hasShareAccess = query.ShareToken is not null && note.SharedLinkToken == query.ShareToken;

        if (!isOwner && !hasShareAccess) return Forbidden;

        var comments = await commentRepository.GetByNoteIdAsync(query.NoteId, cancellationToken);

        var userIds = comments.Select(c => c.UserId).Distinct().ToList();
        var users = await userRepository.GetByIdsAsync(userIds, cancellationToken);
        var userMap = users.ToDictionary(u => u.Id);

        var commentIds = comments.Select(c => c.Id).ToList();
        var likeCounts = await commentRepository.GetLikeCountsAsync(commentIds, cancellationToken);
        var likedByUser = query.UserId is not null
            ? await commentRepository.GetLikedByUserAsync(commentIds, query.UserId, cancellationToken)
            : [];

        var response = comments.Select(c =>
        {
            var user = userMap.GetValueOrDefault(c.UserId);
            return new CommentResponse(
                c.Id.Value,
                c.ParentCommentId?.Value,
                c.UserId.Value,
                user?.Username ?? string.Empty,
                user?.AvatarUrl,
                c.Content,
                likeCounts.GetValueOrDefault(c.Id, 0),
                likedByUser.Contains(c.Id),
                c.CreatedAt,
                c.UpdatedAt);
        }).ToList();

        return Success(new GetCommentsDto(response));
    }
}
