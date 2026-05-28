using Domain.Categories;
using Domain.Notes.Events;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Domain.Notes;

public class Note : AggregateRoot<NoteId>
{
    public static class Errors
    {
        public static readonly Error EmptyTitle  = new("Note.EmptyTitle",  "Title cannot be empty",   ErrorType.Validation);
        public static readonly Error EmptyContent = new("Note.EmptyContent", "Content cannot be empty", ErrorType.Validation);
    }

    private readonly List<NoteId> _linkedNoteIds = [];
    private readonly List<NoteStructure> _structures = [];
    private readonly List<NoteImage> _images = [];

    public UserId UserId { get; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public CategoryId? CategoryId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public SharedLink? SharedLink { get; private set; }

    public IReadOnlyList<NoteId> LinkedNoteIds => _linkedNoteIds;
    public IReadOnlyList<NoteStructure> Structures => _structures;
    public IReadOnlyList<NoteImage> Images => _images;

    private Note(NoteId id, UserId userId, string title, string content) : base(id)
    {
        UserId = userId;
        Title = title;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Note> Create(UserId userId, string title, string content = "")
    {
        if (string.IsNullOrWhiteSpace(title)) return Errors.EmptyTitle;

        var note = new Note(NoteId.New(), userId, title, content);
        note.SyncLinks();
        note.SyncImages();
        return Result.Success(note);
    }

    public Result UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
        SyncLinks();
        SyncImages();
        return Result.Success();
    }

    public Result UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return Errors.EmptyTitle;

        Title = title;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void SetCategory(CategoryId? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public NoteStructure AddStructure(string prompt, string content, string description, IReadOnlyList<(int Index, string Text)> chunks)
    {
        var structure = NoteStructure.Create(Id, prompt, content, description, chunks);
        _structures.Add(structure);
        UpdatedAt = DateTime.UtcNow;
        return structure;
    }

    public SharedLink CreateSharedLink(SharePermission permission)
    {
        SharedLink = SharedLink.Create(permission);
        UpdatedAt = DateTime.UtcNow;
        return SharedLink;
    }

    public void DeleteSharedLink()
    {
        SharedLink = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        var imageUrls = _images.Select(img => img.PublicUrl).ToList();
        RaiseDomainEvent(new NoteDeletedEvent(Id, imageUrls));
    }

    private void SyncLinks()
    {
        var parsed = NoteParser.ParseNoteLinks(Content);
        var next = parsed.Select(g => new NoteId(g)).ToList();
        var diff = NoteLinkDiffer.Diff(_linkedNoteIds, next);

        foreach (var id in diff.ToRemove)
            _linkedNoteIds.Remove(id);

        _linkedNoteIds.AddRange(diff.ToAdd);
    }

    private void SyncImages()
    {
        var parsed = NoteImageParser.ParseImageUrls(Content).ToHashSet();

        var toDisable = _images.Where(img => img.Enable && !parsed.Contains(img.PublicUrl)).ToList();
        foreach (var img in toDisable)
            img.Disable();

        var existingUrls = _images.Select(img => img.PublicUrl).ToHashSet();
        foreach (var url in parsed.Where(url => !existingUrls.Contains(url)))
            _images.Add(NoteImage.Create(Id, url));

        if (toDisable.Count > 0)
            RaiseDomainEvent(new NoteImagesChangedEvent(Id, toDisable.Select(img => img.PublicUrl).ToList()));
    }
}
