using CoreMesh.Outbox.Abstractions;

namespace ShareKernal;

public interface IAggregateRoot
{
    IReadOnlyList<IEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
