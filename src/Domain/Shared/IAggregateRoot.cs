using CoreMesh.Outbox.Abstractions;

namespace Domain.Shared;

public interface IAggregateRoot
{
    IReadOnlyList<IEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
