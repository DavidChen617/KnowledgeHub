using CoreMesh.Outbox.Abstractions;
using Domain.Notes;
using Domain.Notes.Events;

namespace Application.EventHandlers;

public sealed class NoteImagesChangedEventHandler(IImageStorage imageStorage) : IEventHandler<NoteImagesChangedEvent>
{
    public async Task HandleAsync(NoteImagesChangedEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event.DisabledUrls.Count == 0)
            return;

        await imageStorage.DeleteManyAsync(@event.DisabledUrls, cancellationToken);
    }
}
