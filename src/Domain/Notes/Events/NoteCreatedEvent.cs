using CoreMesh.Outbox.Abstractions;

namespace Domain.Notes.Events;

[EventName("note.created")]
public record NoteCreatedEvent(Guid NoteId, Guid UserId) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
