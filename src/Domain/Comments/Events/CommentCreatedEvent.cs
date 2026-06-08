using CoreMesh.Outbox.Abstractions;

namespace Domain.Comments.Events;

[EventName("comment.created")]
public record CommentCreatedEvent(Guid CommentId, Guid NoteId, Guid UserId, Guid? ParentCommentId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
