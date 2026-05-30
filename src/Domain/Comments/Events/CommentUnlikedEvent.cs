using CoreMesh.Outbox.Abstractions;

namespace Domain.Comments.Events;

[EventName("comment.unliked")]
public record CommentUnlikedEvent(Guid CommentId, Guid NoteId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
