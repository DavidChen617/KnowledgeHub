using CoreMesh.Outbox.Abstractions;

namespace Domain.Categories.Events;

[EventName("category.updated")]
public record CategoryUpdatedEvent(Guid CategoryId, Guid UserId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
