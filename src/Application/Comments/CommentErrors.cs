using ShareKernal;

namespace Application.Comments;

public static class CommentErrors
{
    public static readonly Error NotFound    = new("Comment.NotFound",    "Comment not found", ErrorType.NotFound);
    public static readonly Error Forbidden   = new("Comment.Forbidden",   "Access denied",     ErrorType.Forbidden);
    public static readonly Error AlreadyLiked = new("Comment.AlreadyLiked", "Comment already liked", ErrorType.Conflict);
}
