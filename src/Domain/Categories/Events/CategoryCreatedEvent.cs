using CoreMesh.Outbox.Abstractions;

namespace Domain.Categories.Events;

[EventName("category.created")]
public record CategoryCreatedEvent(Guid CategoryId, Guid UserId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
