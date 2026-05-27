using CoreMesh.Outbox.Abstractions;

namespace Domain.Notes.Events;

[EventName("note.images-changed")]
public record NoteImagesChangedEvent(NoteId NoteId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
