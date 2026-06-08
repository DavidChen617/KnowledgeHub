using CoreMesh.Outbox.Abstractions;

namespace Domain.Shared;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyList<IEvent> DomainEvents => _domainEvents;

    protected void RaiseDomainEvent(IEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
