using Application.Notes;
using Domain.Notes;
using Domain.Notes.Events;
using Domain.Users;
using ShareKernal;

namespace UnitTests.Notes;

public class NoteDeleteTests
{
    [Fact]
    public async Task Given_NoteNotFound_When_DeleteNote_Then_ReturnsNotFound()
    {
        var repo = new FakeNoteRepository(returnNote: null);
        var handler = new DeleteNoteHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new DeleteNoteCommand(NoteId.New(), UserId.New()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.False(repo.WasDeleted);
    }

    [Fact]
    public async Task Given_NoteOwner_When_DeleteNote_Then_SucceedsAndRaisesEvent()
    {
        var userId = UserId.New();
        var note = Note.Create(userId, "標題", "內容").Value;
        var repo = new FakeNoteRepository(note);
        var handler = new DeleteNoteHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new DeleteNoteCommand(note.Id, userId));

        Assert.True(result.IsSuccess);
        Assert.True(repo.WasDeleted);
        Assert.Single(note.DomainEvents.OfType<NoteDeletedEvent>());
    }

    [Fact]
    public async Task Given_NoteWithImages_When_DeleteNote_Then_EventContainsImageUrls()
    {
        var userId = UserId.New();
        var imageUrl = "https://res.cloudinary.com/test/image/upload/v1/arch.png";
        var note = Note.Create(userId, "標題", $"![圖]({imageUrl})").Value;
        var repo = new FakeNoteRepository(note);
        var handler = new DeleteNoteHandler(repo, FakeUnitOfWork.Instance);

        await handler.Handle(new DeleteNoteCommand(note.Id, userId));

        var ev = Assert.Single(note.DomainEvents.OfType<NoteDeletedEvent>());
        Assert.Contains(imageUrl, ev.ImageUrls);
    }
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public static readonly FakeUnitOfWork Instance = new();
    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
}

file class FakeNoteRepository(Note? returnNote) : INoteRepository
{
    public bool WasDeleted { get; private set; }
    public Note? DeletedNote { get; private set; }

    public Task AddAsync(Note note, CancellationToken ct = default) => Task.CompletedTask;
    public Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default) => Task.FromResult(returnNote);
    public Task<Note?> GetByIdAndUserIdAsync(NoteId id, UserId userId, CancellationToken ct = default) => Task.FromResult(returnNote);
    public Task Update(Note note, CancellationToken ct = default) => Task.CompletedTask;
    public Task<IReadOnlyList<Note>> GetAllByUserIdAsync(UserId userId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Note>>([]);
    public Task<IReadOnlyList<Note>> SearchByTitleAsync(UserId userId, string title, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Note>>([]);
    public Task DeleteAsync(Note note, CancellationToken ct = default)
    {
        WasDeleted = true;
        DeletedNote = note;
        return Task.CompletedTask;
    }
    public Task<Note?> GetBySharedTokenAsync(string token, CancellationToken ct = default) => Task.FromResult<Note?>(null);
}
