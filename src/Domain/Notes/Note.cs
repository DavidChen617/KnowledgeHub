using Domain.Categories;
using Domain.Notes.Events;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Domain.Notes;

public sealed class NoteId : ValueObject
{
    public Guid Value { get; }
    public NoteId() => Value = Guid.NewGuid();
    public NoteId(Guid value) => Value = value;
    public static NoteId New() => new(Guid.NewGuid());
    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}

public class Note : AggregateRoot<NoteId>
{
    public static class Errors
    {
        public static readonly Error EmptyTitle = new("Note.EmptyTitle", "Title cannot be empty", ErrorType.Validation);
    }

    private readonly List<NoteStructure> _structures = [];
    private readonly List<NoteImage> _images = [];

    public UserId UserId { get; }
    public string Title { get; private set; }
    public NoteContent Content { get; private set; }
    public CategoryId? CategoryId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public SharedLink? SharedLink { get; private set; }

    public IReadOnlyList<NoteId> LinkedNoteIds => Content.LinkedNoteIds;
    public IReadOnlyList<NoteStructure> Structures => _structures;
    public IReadOnlyList<NoteImage> Images => _images;

    private Note(NoteId id, UserId userId, string title, NoteContent content) : base(id)
    {
        UserId = userId;
        Title = title;
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Note> Create(UserId userId, string title, string content = "")
    {
        if (string.IsNullOrWhiteSpace(title)) return Errors.EmptyTitle;

        var note = new Note(NoteId.New(), userId, title, new NoteContent(content));
        note.SyncImages();
        return Result.Success(note);
    }

    public Result UpdateContent(string content)
    {
        Content = new NoteContent(content);
        UpdatedAt = DateTime.UtcNow;
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

    public NoteStructure AddStructure(string prompt, string content, string description)
    {
        var chunks = ChunkByHeadings(content);
        var structure = NoteStructure.Create(Id, prompt, content, description, chunks);
        _structures.Add(structure);
        UpdatedAt = DateTime.UtcNow;
        return structure;
    }

    private static IReadOnlyList<(int, string)> ChunkByHeadings(string content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        var chunks = new List<(int, string)>();
        var currentLines = new List<string>();
        var index = 0;

        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith("### ") && currentLines.Count > 0)
            {
                chunks.Add((index++, string.Join('\n', currentLines).Trim()));
                currentLines.Clear();
            }
            currentLines.Add(line);
        }

        if (currentLines.Count > 0)
        {
            var text = string.Join('\n', currentLines).Trim();
            if (!string.IsNullOrEmpty(text))
                chunks.Add((index, text));
        }

        return chunks;
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

    private void SyncImages()
    {
        var parsed = Content.ImageUrls.ToHashSet();

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
