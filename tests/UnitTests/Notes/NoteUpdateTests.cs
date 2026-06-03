using Application.Notes;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace UnitTests.Notes;

public class NoteUpdateTests
{
    [Fact]
    public async Task Given_NoteNotFound_When_UpdateNote_Then_ReturnsNotFound()
    {
        var repo = new FakeNoteRepository(returnNote: null);
        var handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new UpdateNoteCommandRequest(NoteId.New(), UserId.New(), "新標題", null, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.False(repo.WasUpdated);
    }

    [Fact]
    public async Task Given_NoteExists_When_UpdateTitleOnly_Then_TitleUpdated()
    {
        var userId = UserId.New();
        var note = Note.Create(userId, "原始標題", "內容").Value;
        var repo = new FakeNoteRepository(note);
        var handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new UpdateNoteCommandRequest(note.Id, userId, "新標題", null, null));

        Assert.True(result.IsSuccess);
        Assert.Equal("新標題", result.Value.Title);
        Assert.True(repo.WasUpdated);
    }

    [Fact]
    public async Task Given_ContentWithImages_When_UpdateContentRemovesImage_Then_ImageDisabled()
    {
        var userId = UserId.New();
        var imageUrl = "https://res.cloudinary.com/test/image/upload/v1/diagram.png";
        var note = Note.Create(userId, "標題", $"![圖]({imageUrl})").Value;
        var repo = new FakeNoteRepository(note);
        var handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new UpdateNoteCommandRequest(note.Id, userId, null, "移除圖片的新內容", null));

        Assert.True(result.IsSuccess);
        Assert.False(note.Images.Single(img => img.PublicUrl == imageUrl).Enable);
    }

    [Fact]
    public async Task Given_ContentWithLinks_When_UpdateContentRemovesLink_Then_LinkedNoteIdsEmpty()
    {
        var userId = UserId.New();
        var refId = Guid.NewGuid();
        var note = Note.Create(userId, "標題", $"參考 [[{refId}]]").Value;
        var repo = new FakeNoteRepository(note);
        var handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new UpdateNoteCommandRequest(note.Id, userId, null, "不含連結的新內容", null));

        Assert.True(result.IsSuccess);
        Assert.Empty(note.LinkedNoteIds);
    }

    [Fact]
    public async Task Given_NoteExists_When_UpdateTitleAndContent_Then_BothUpdated()
    {
        var userId = UserId.New();
        var note = Note.Create(userId, "原始標題", "原始內容").Value;
        var repo = new FakeNoteRepository(note);
        var handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);

        var result = await handler.Handle(new UpdateNoteCommandRequest(note.Id, userId, "最終標題", "最終內容", null));

        Assert.True(result.IsSuccess);
        Assert.Equal("最終標題", result.Value.Title);
        Assert.Equal("最終內容", result.Value.Content);
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
    public bool WasUpdated { get; private set; }

    public Task AddAsync(Note note, CancellationToken ct = default) => Task.CompletedTask;
    public Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default) => Task.FromResult(returnNote);
    public Task<Note?> GetByIdAndUserIdAsync(NoteId id, UserId userId, CancellationToken ct = default) => Task.FromResult(returnNote);
    public Task Update(Note note, CancellationToken ct = default) { WasUpdated = true; return Task.CompletedTask; }
    public Task<IReadOnlyList<Note>> GetAllByUserIdAsync(UserId userId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Note>>([]);
    public Task<IReadOnlyList<Note>> SearchByTitleAsync(UserId userId, string title, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Note>>([]);
    public Task DeleteAsync(Note note, CancellationToken ct = default) => Task.CompletedTask;
    public Task<Note?> GetBySharedTokenAsync(string token, CancellationToken ct = default) => Task.FromResult<Note?>(null);
}
