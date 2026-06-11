using CoreMesh.Outbox.Abstractions;

namespace Domain.Users.Events;

[EventName("user.registered")]
public record UserRegisteredEvent(Guid UserId, string Email) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
