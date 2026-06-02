using Application.Notes;
using Domain.Notes;
using Domain.Notes.Events;
using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace UnitTests.Notes;

public class NoteUpdateTests
{
    [Fact]
    public async Task UpdateNote_UseCase()
    {
        var refId = Guid.NewGuid();
        var imageUrl = "https://res.cloudinary.com/test/image/upload/v1/diagram.png";
        var userId = UserId.New();

        // --- 1. 建立筆記（含連結與圖片）---
        var created = Note.Create(userId, "原始標題", $"""
            學習筆記，參考 [[{refId}]]
            ![圖解]({imageUrl})
            """);

        Assert.True(created.IsSuccess);
        var note = created.Value;
        Assert.Equal("原始標題", note.Title);
        Assert.Single(note.LinkedNoteIds);
        Assert.Single(note.Images);
        Assert.True(note.Images[0].Enable);
        Console.WriteLine($"[1] 建立筆記，連結數：{note.LinkedNoteIds.Count}，圖片數：{note.Images.Count}");

        // --- 2. 非擁有者更新，回傳 null ---
        var repo = new FakeNoteRepository(returnNote: null);
        var handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);
        var wrongCommand = new UpdateNoteCommandRequest(note.Id, UserId.New(), "新標題", null, null);

        var notFoundResult = await handler.Handle(wrongCommand);

        Assert.False(notFoundResult.IsSuccess);
        Assert.Equal(ErrorType.NotFound, notFoundResult.Error.Type);
        Assert.False(repo.WasUpdated);
        Console.WriteLine($"[2] 非擁有者更新：回傳 NotFound");

        // --- 3. 只更新標題 ---
        repo = new FakeNoteRepository(returnNote: note);
        handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);
        var linkedCountBefore = note.LinkedNoteIds.Count;
        var titleCommand = new UpdateNoteCommandRequest(note.Id, userId, "新標題", null, null);

        var titleResult = await handler.Handle(titleCommand);

        Assert.True(titleResult.IsSuccess);
        Assert.Equal("新標題", titleResult.Value.Title);
        Assert.True(repo.WasUpdated);
        Assert.Equal(linkedCountBefore, note.LinkedNoteIds.Count);
        Console.WriteLine($"[3] 只更新標題：{titleResult.Value.Title}");

        // --- 4. 更新內容：移除圖片、移除連結 ---
        repo = new FakeNoteRepository(returnNote: note);
        handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);
        var contentCommand = new UpdateNoteCommandRequest(note.Id, userId, null, "重寫內容，不含圖片與連結", null);

        var contentResult = await handler.Handle(contentCommand);

        Assert.True(contentResult.IsSuccess);
        Assert.Equal("重寫內容，不含圖片與連結", contentResult.Value.Content);

        Assert.Empty(note.LinkedNoteIds);

        var disabledImage = note.Images.Single(img => img.PublicUrl == imageUrl);
        Assert.False(disabledImage.Enable);
        Assert.Single(note.DomainEvents.OfType<NoteImagesChangedEvent>());

        Console.WriteLine($"[4] 更新內容：連結清空，圖片 disable {note.Images.Count(img => !img.Enable)} 張");

        // --- 5. 同時更新標題與內容 ---
        repo = new FakeNoteRepository(returnNote: note);
        handler = new UpdateNoteHandler(repo, FakeUnitOfWork.Instance);
        var bothCommand = new UpdateNoteCommandRequest(note.Id, userId, "最終標題", "最終內容", null);

        var bothResult = await handler.Handle(bothCommand);

        Assert.True(bothResult.IsSuccess);
        Assert.Equal("最終標題", bothResult.Value.Title);
        Assert.Equal("最終內容", bothResult.Value.Content);
        Console.WriteLine($"[5] 同時更新：標題={bothResult.Value.Title}，內容={bothResult.Value.Content}");
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

    public Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default) =>
        Task.FromResult(returnNote);

    public Task<Note?> GetByIdAndUserIdAsync(NoteId id, UserId userId, CancellationToken ct = default) =>
        Task.FromResult(returnNote);

    public Task Update(Note note, CancellationToken ct = default)
    {
        WasUpdated = true;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Note>> GetAllByUserIdAsync(UserId userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Note>>([]);

    public Task<IReadOnlyList<Note>> SearchByTitleAsync(UserId userId, string title, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Note>>([]);

    public Task DeleteAsync(Note note, CancellationToken ct = default) => Task.CompletedTask;

    public Task<Note?> GetBySharedTokenAsync(string token, CancellationToken ct = default) =>
        Task.FromResult<Note?>(null);
}
