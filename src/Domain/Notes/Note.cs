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
    public string? SharedLinkToken { get; private set; }

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
        note.RaiseDomainEvent(new NoteCreatedEvent(note.Id.Value, userId.Value));
        return Result.Success(note);
    }

    public Result UpdateContent(string content)
    {
        Content = new NoteContent(content);
        UpdatedAt = DateTime.UtcNow;
        SyncImages();
        RaiseDomainEvent(new NoteUpdatedEvent(Id.Value, UserId.Value, SharedLinkToken));
        return Result.Success();
    }

    public Result UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return Errors.EmptyTitle;

        Title = title;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new NoteUpdatedEvent(Id.Value, UserId.Value, SharedLinkToken));
        return Result.Success();
    }

    public void SetCategory(CategoryId? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new NoteUpdatedEvent(Id.Value, UserId.Value, SharedLinkToken));
    }

    public NoteStructure AddStructure(string prompt, string content, string description)
    {
        var chunks = ChunkByHeadings(content);
        var structure = NoteStructure.Create(Id, prompt, content, description, chunks);
        _structures.Add(structure);
        UpdatedAt = DateTime.UtcNow;
        return structure;
    }

    public string CreateSharedLink()
    {
        var token = GenerateToken();
        SharedLinkToken = token;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SharedLinkCreatedEvent(Id.Value, UserId.Value, SharedLinkToken));
        return token;
    }

    public void DeleteSharedLink()
    {
        var token = SharedLinkToken;
        SharedLinkToken = null;
        UpdatedAt = DateTime.UtcNow;
        if (token is not null)
            RaiseDomainEvent(new SharedLinkDeletedEvent(Id.Value, UserId.Value, token));
    }

    public void Delete()
    {
        var imageUrls = _images.Select(img => img.PublicUrl).ToList();
        RaiseDomainEvent(new NoteDeletedEvent(Id.Value, UserId.Value, imageUrls));
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

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
}
