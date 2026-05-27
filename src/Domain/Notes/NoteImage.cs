using Domain.Shared;

namespace Domain.Notes;

public class NoteImage : Entity<Guid>
{
    public NoteId NoteId { get; }
    public string PublicUrl { get; }
    public bool Enable { get; private set; }

    private NoteImage(Guid id, NoteId noteId, string publicUrl) : base(id)
    {
        NoteId = noteId;
        PublicUrl = publicUrl;
        Enable = true;
    }

    public static NoteImage Create(NoteId noteId, string publicUrl) =>
        new(Guid.NewGuid(), noteId, publicUrl);

    public void Disable() => Enable = false;
}
