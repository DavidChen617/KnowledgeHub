using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Domain.Comments;

public class Comment : AggregateRoot<CommentId>
{
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

    public static Comment Create(NoteId noteId, UserId userId, string content, CommentId? parentCommentId = null) =>
        new(CommentId.New(), noteId, userId, parentCommentId, content);

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
