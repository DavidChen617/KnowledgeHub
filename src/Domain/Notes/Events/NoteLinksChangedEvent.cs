using CoreMesh.Outbox.Abstractions;

namespace Domain.Notes.Events;

public record NoteLinksChangedEvent(
    NoteId NoteId,
    IReadOnlyList<NoteId> ToAdd,
    IReadOnlyList<NoteId> ToRemove
) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
