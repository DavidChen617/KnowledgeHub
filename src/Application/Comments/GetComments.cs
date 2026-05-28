using CoreMesh.Dispatching.Abstractions;
using Domain.Comments;
using Domain.Notes;
using Domain.Users;

namespace Application.Comments;

public record GetCommentsQueryRequest(NoteId NoteId, UserId? UserId, string? ShareToken) : IRequest<GetCommentsQueryResponse?>;

public record CommentResponse(
    Guid CommentId,
    Guid? ParentCommentId,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Content,
    int LikeCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetCommentsQueryResponse(IReadOnlyList<CommentResponse> Comments);

public class GetCommentsHandler(
    INoteRepository noteRepository,
    ICommentRepository commentRepository,
    IUserRepository userRepository)
    : IRequestHandler<GetCommentsQueryRequest, GetCommentsQueryResponse?>
{
    public async Task<GetCommentsQueryResponse?> Handle(GetCommentsQueryRequest query, CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(query.NoteId, cancellationToken);
        if (note is null) return null;

        var isOwner = query.UserId is not null && note.UserId == query.UserId;
        var hasShareAccess = query.ShareToken is not null && note.SharedLink?.Token == query.ShareToken;

        if (!isOwner && !hasShareAccess) return null;

        var comments = await commentRepository.GetByNoteIdAsync(query.NoteId, cancellationToken);

        var userIds = comments.Select(c => c.UserId).Distinct().ToList();
        var users = await Task.WhenAll(userIds.Select(id => userRepository.GetByIdAsync(id, cancellationToken)));
        var userMap = users.Where(u => u is not null).ToDictionary(u => u!.Id, u => u!);

        var likeCounts = await commentRepository.GetLikeCountsAsync(comments.Select(c => c.Id), cancellationToken);

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
                c.CreatedAt,
                c.UpdatedAt);
        }).ToList();

        return new GetCommentsQueryResponse(response);
    }
}
