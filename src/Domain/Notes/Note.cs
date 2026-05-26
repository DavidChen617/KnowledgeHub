using Domain.Notes.Events;
using Domain.Shared;
using Domain.Users;

namespace Domain.Notes;

public class Note : AggregateRoot<NoteId>
{
    private readonly List<NoteId> _linkedNoteIds = [];
    private readonly List<NoteStructure> _structures = [];

    public UserId UserId { get; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public SharedLink? SharedLink { get; private set; }

    public IReadOnlyList<NoteId> LinkedNoteIds => _linkedNoteIds;
    public IReadOnlyList<NoteStructure> Structures => _structures;

    private Note(NoteId id, UserId userId, string title, string content) : base(id)
    {
        UserId = userId;
        Title = title;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Note Create(UserId userId, string title, string content = "")
    {
        var note = new Note(NoteId.New(), userId, title, content);
        note.SyncLinks();
        return note;
    }

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
        SyncLinks();
    }

    public void UpdateTitle(string title)
    {
        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public NoteStructure AddStructure(string prompt, string content, IReadOnlyList<(int Index, string Text)> chunks)
    {
        var structure = NoteStructure.Create(Id, prompt, content, chunks);
        _structures.Add(structure);
        UpdatedAt = DateTime.UtcNow;
        return structure;
    }

    public SharedLink CreateSharedLink()
    {
        SharedLink = SharedLink.Create();
        UpdatedAt = DateTime.UtcNow;
        return SharedLink;
    }

    private void SyncLinks()
    {
        var parsed = NoteParser.ParseNoteLinks(Content);
        var next = parsed.Select(g => new NoteId(g)).ToList();
        var diff = NoteLinkDiffer.Diff(_linkedNoteIds, next);

        foreach (var id in diff.ToRemove)
            _linkedNoteIds.Remove(id);

        _linkedNoteIds.AddRange(diff.ToAdd);

        if (diff.ToAdd.Count > 0 || diff.ToRemove.Count > 0)
            RaiseDomainEvent(new NoteLinksChangedEvent(Id, diff.ToAdd, diff.ToRemove));
    }
}
