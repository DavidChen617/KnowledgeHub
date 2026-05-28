using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Domain.Comments;

public class Comment : AggregateRoot<CommentId>
{
    public static class Errors
    {
        public static readonly Error EmptyContent = new("Comment.EmptyContent", "Content cannot be empty", ErrorType.Validation);
    }

    public NoteId NoteId { get; }
    public UserId UserId { get; }
    public CommentId? ParentCommentId { get; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    private Comment(CommentId id, NoteId noteId, UserId userId, CommentId? parentCommentId, string content)
        : base(id)
    {
        NoteId = noteId;
        UserId = userId;
        ParentCommentId = parentCommentId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Comment> Create(NoteId noteId, UserId userId, string content, CommentId? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(content)) return Errors.EmptyContent;
        return Result.Success(new Comment(CommentId.New(), noteId, userId, parentCommentId, content));
    }

    public Result UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Errors.EmptyContent;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
