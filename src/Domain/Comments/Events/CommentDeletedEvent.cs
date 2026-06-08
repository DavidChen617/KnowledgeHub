using CoreMesh.Outbox.Abstractions;

namespace Domain.Comments.Events;

[EventName("comment.deleted")]
public record CommentDeletedEvent(Guid CommentId, Guid NoteId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
