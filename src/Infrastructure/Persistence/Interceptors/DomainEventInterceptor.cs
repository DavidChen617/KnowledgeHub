using CoreMesh.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShareKernal;

namespace Infrastructure.Persistence.Interceptors;

public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ConvertDomainEventsToOutboxMessages(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDomainEventsToOutboxMessages(DbContext context)
    {
        var aggregates = context.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IAggregateRoot>()
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var messages = aggregates
            .SelectMany(a => a.DomainEvents)
            .Select(OutboxMessage.Create)
            .ToList();

        context.Set<OutboxMessage>().AddRange(messages);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }
}
