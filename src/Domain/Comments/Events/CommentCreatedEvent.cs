using CoreMesh.Outbox.Abstractions;
using Domain.Notes;
using Domain.Users;

namespace Domain.Comments.Events;

[EventName("comment.created")]
public record CommentCreatedEvent(CommentId CommentId, NoteId NoteId, UserId UserId, CommentId? ParentCommentId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
