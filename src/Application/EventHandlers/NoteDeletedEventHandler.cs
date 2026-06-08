using CoreMesh.Outbox.Abstractions;
using Domain.Notes;
using Domain.Notes.Events;

namespace Application.EventHandlers;

public sealed class NoteDeletedEventHandler(IImageStorage imageStorage) : IEventHandler<NoteDeletedEvent>
{
    public async Task HandleAsync(NoteDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event.ImageUrls.Count == 0)
            return;

        await imageStorage.DeleteManyAsync(@event.ImageUrls, cancellationToken);
    }
}
