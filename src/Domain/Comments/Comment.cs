using Domain.Notes;
using Domain.Shared;
using Domain.Users;

namespace Domain.Comments;

public class Comment : AggregateRoot<CommentId>
{
    public NoteId NoteId { get; }
    public UserId AuthorId { get; }
    public string Content { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Comment(CommentId id, NoteId noteId, UserId authorId, string content) : base(id)
    {
        NoteId = noteId;
        AuthorId = authorId;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Comment Create(NoteId noteId, UserId authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("留言內容不得為空");

        return new Comment(CommentId.New(), noteId, authorId, content);
    }
}
