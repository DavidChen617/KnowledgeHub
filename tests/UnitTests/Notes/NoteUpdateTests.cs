using Application.Notes;
using Domain.Notes;
using Domain.Notes.Events;
using Domain.Users;

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
        var note = Note.Create(userId, "原始標題", $"""
            學習筆記，參考 [[{refId}]]
            ![圖解]({imageUrl})
            """);

        Assert.Equal("原始標題", note.Title);
        Assert.Single(note.LinkedNoteIds);
        Assert.Single(note.Images);
        Assert.True(note.Images[0].Enable);
        Console.WriteLine($"[1] 建立筆記，連結數：{note.LinkedNoteIds.Count}，圖片數：{note.Images.Count}");

        // --- 2. 非擁有者更新，回傳 null ---
        var repo = new FakeNoteRepository(returnNote: null);
        var handler = new UpdateNoteHandler(repo);
        var wrongCommand = new UpdateNoteCommandRequest(note.Id, UserId.New(), "新標題", null);

        var notFoundResult = await handler.Handle(wrongCommand);

        Assert.Null(notFoundResult);
        Assert.False(repo.WasUpdated);
        Console.WriteLine($"[2] 非擁有者更新：回傳 null");

        // --- 3. 只更新標題 ---
        repo = new FakeNoteRepository(returnNote: note);
        handler = new UpdateNoteHandler(repo);
        var linksEventCountBefore = note.DomainEvents.OfType<NoteLinksChangedEvent>().Count();
        var titleCommand = new UpdateNoteCommandRequest(note.Id, userId, "新標題", null);

        var titleResult = await handler.Handle(titleCommand);

        Assert.NotNull(titleResult);
        Assert.Equal("新標題", titleResult.Title);
        Assert.True(repo.WasUpdated);
        Assert.Equal(linksEventCountBefore, note.DomainEvents.OfType<NoteLinksChangedEvent>().Count());
        Console.WriteLine($"[3] 只更新標題：{titleResult.Title}");

        // --- 4. 更新內容：移除圖片、移除連結 ---
        repo = new FakeNoteRepository(returnNote: note);
        handler = new UpdateNoteHandler(repo);
        var contentCommand = new UpdateNoteCommandRequest(note.Id, userId, null, "重寫內容，不含圖片與連結");

        var contentResult = await handler.Handle(contentCommand);

        Assert.NotNull(contentResult);
        Assert.Equal("重寫內容，不含圖片與連結", contentResult.Content);

        var linksEvent = note.DomainEvents.OfType<NoteLinksChangedEvent>().Last();
        Assert.Contains(new NoteId(refId), linksEvent.ToRemove);

        var disabledImage = note.Images.Single(img => img.PublicUrl == imageUrl);
        Assert.False(disabledImage.Enable);
        Assert.Single(note.DomainEvents.OfType<NoteImagesChangedEvent>());

        Console.WriteLine($"[4] 更新內容：連結移除 {linksEvent.ToRemove.Count} 個，圖片 disable {note.Images.Count(img => !img.Enable)} 張");

        // --- 5. 同時更新標題與內容 ---
        repo = new FakeNoteRepository(returnNote: note);
        handler = new UpdateNoteHandler(repo);
        var bothCommand = new UpdateNoteCommandRequest(note.Id, userId, "最終標題", "最終內容");

        var bothResult = await handler.Handle(bothCommand);

        Assert.NotNull(bothResult);
        Assert.Equal("最終標題", bothResult.Title);
        Assert.Equal("最終內容", bothResult.Content);
        Console.WriteLine($"[5] 同時更新：標題={bothResult.Title}，內容={bothResult.Content}");
    }
}

file class FakeNoteRepository(Note? returnNote) : INoteRepository
{
    public bool WasUpdated { get; private set; }

    public Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default) =>
        Task.FromResult(returnNote);

    public Task<Note?> GetByIdAndUserIdAsync(NoteId id, UserId userId, CancellationToken ct = default) =>
        Task.FromResult(returnNote);

    public Task UpdateAsync(Note note, CancellationToken ct = default)
    {
        WasUpdated = true;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Note note, CancellationToken ct = default) => Task.CompletedTask;
}
