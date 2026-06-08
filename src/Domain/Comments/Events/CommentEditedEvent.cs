using CoreMesh.Outbox.Abstractions;

namespace Domain.Comments.Events;

[EventName("comment.edited")]
public record CommentEditedEvent(Guid CommentId, Guid NoteId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
