using CoreMesh.Outbox.Abstractions;

namespace Domain.Notes.Events;

[EventName("note.shared-link.deleted")]
public record SharedLinkDeletedEvent(Guid NoteId, Guid UserId, string Token) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
