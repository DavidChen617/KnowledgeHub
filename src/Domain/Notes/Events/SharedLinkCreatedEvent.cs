using CoreMesh.Outbox.Abstractions;

namespace Domain.Notes.Events;

[EventName("note.shared-link.created")]
public record SharedLinkCreatedEvent(Guid NoteId, Guid UserId, string? PreviousToken) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
