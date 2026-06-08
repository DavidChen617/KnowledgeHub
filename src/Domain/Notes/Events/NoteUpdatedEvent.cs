using CoreMesh.Outbox.Abstractions;

namespace Domain.Notes.Events;

[EventName("note.updated")]
public record NoteUpdatedEvent(Guid NoteId, Guid UserId, string? SharedLinkToken) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
