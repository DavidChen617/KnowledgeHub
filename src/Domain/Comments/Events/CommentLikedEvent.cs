using CoreMesh.Outbox.Abstractions;

namespace Domain.Comments.Events;

[EventName("comment.liked")]
public record CommentLikedEvent(Guid CommentId, Guid UserId, Guid NoteId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
