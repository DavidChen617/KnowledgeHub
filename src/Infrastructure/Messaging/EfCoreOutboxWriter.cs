using CoreMesh.Outbox.Abstractions;
using Infrastructure.Persistence;

namespace Infrastructure.Messaging;

internal sealed class EfCoreOutboxWriter(AppDbContext db) : IOutboxWriter
{
    public async Task AddAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessage.Create(@event);
        await db.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
