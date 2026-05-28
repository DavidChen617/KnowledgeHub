using CoreMesh.Outbox.Abstractions;

namespace Domain.Notes.Events;

[EventName("note.deleted")]
public record NoteDeletedEvent(NoteId NoteId, IReadOnlyList<string> ImageUrls) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
