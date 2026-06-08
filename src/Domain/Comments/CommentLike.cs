using Domain.Users;

namespace Domain.Comments;

public class CommentLike
{
    public CommentId CommentId { get; }
    public UserId UserId { get; }
    public DateTime CreatedAt { get; }

    private CommentLike(CommentId commentId, UserId userId)
    {
        CommentId = commentId;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public static CommentLike Create(CommentId commentId, UserId userId) => new(commentId, userId);
}
