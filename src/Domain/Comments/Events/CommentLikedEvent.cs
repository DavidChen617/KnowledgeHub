using CoreMesh.Outbox.Abstractions;
using Domain.Users;

namespace Domain.Comments.Events;

[EventName("comment.liked")]
public record CommentLikedEvent(CommentId CommentId, UserId UserId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
