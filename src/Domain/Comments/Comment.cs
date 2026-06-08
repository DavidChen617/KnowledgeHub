using Domain.Comments.Events;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Domain.Comments;

public sealed class CommentId : ValueObject
{
    public Guid Value { get; }
    public CommentId(Guid value) => Value = value;
    public static CommentId New() => new(Guid.NewGuid());
    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}

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
    public DateTime UpdatedAt { get; private set; }

    private Comment(CommentId id, NoteId noteId, UserId userId, CommentId? parentCommentId, string content)
        : base(id)
    {
        NoteId = noteId;
        UserId = userId;
        ParentCommentId = parentCommentId;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Comment> Create(NoteId noteId, UserId userId, string content, CommentId? parentCommentId = null)
    {
        if (string.IsNullOrWhiteSpace(content)) return Errors.EmptyContent;
        var comment = new Comment(CommentId.New(), noteId, userId, parentCommentId, content);
        comment.RaiseDomainEvent(new CommentCreatedEvent(comment.Id.Value, noteId.Value, userId.Value, parentCommentId?.Value));
        return Result.Success(comment);
    }

    public Result UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Errors.EmptyContent;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new CommentEditedEvent(Id.Value, NoteId.Value));
        return Result.Success();
    }

    public CommentLike Like(UserId userId)
    {
        RaiseDomainEvent(new CommentLikedEvent(Id.Value, userId.Value, NoteId.Value));
        return CommentLike.Create(Id, userId);
    }

    public void Unlike()
    {
        RaiseDomainEvent(new CommentUnlikedEvent(Id.Value, NoteId.Value));
    }

    public void Delete()
    {
        RaiseDomainEvent(new CommentDeletedEvent(Id.Value, NoteId.Value));
    }
}
