using Application.Notes;
using Domain.Notes;
using Domain.Notes.Events;
using Domain.Users;

namespace UnitTests.Notes;

public class NoteDeleteTests
{
    [Fact]
    public async Task DeleteNote_UseCase()
    {
        // --- 1. 建立含圖片的筆記 ---
        var content = """
            # 系統設計筆記
            架構圖如下：
            ![架構圖](https://res.cloudinary.com/test/image/upload/v1/arch.png)
            詳細說明請參考：
            ![流程圖](https://res.cloudinary.com/test/image/upload/v1/flow.png)
            """;

        var userId = UserId.New();
        var note = Note.Create(userId, "系統設計筆記", content);

        Assert.Equal(2, note.Images.Count);
        Assert.All(note.Images, img => Assert.True(img.Enable));
        Console.WriteLine($"[1] 建立筆記，追蹤圖片數：{note.Images.Count}");

        // --- 2. 更新內容，移除一張圖片 ---
        var updatedContent = """
            # 系統設計筆記
            架構圖如下：
            ![架構圖](https://res.cloudinary.com/test/image/upload/v1/arch.png)
            流程圖已移除。
            """;

        note.UpdateContent(updatedContent);

        var imagesChangedEvent = Assert.Single(note.DomainEvents.OfType<NoteImagesChangedEvent>());
        Assert.Equal(note.Id, imagesChangedEvent.NoteId);

        var disabledImage = note.Images.Single(img => !img.Enable);
        Assert.Equal("https://res.cloudinary.com/test/image/upload/v1/flow.png", disabledImage.PublicUrl);
        Console.WriteLine($"[2] 移除圖片：{disabledImage.PublicUrl}，NoteImagesChangedEvent 觸發");

        // --- 3. 使用者非擁有者，handler 回傳 null ---
        var wrongUserRepo = new FakeNoteRepository(returnNote: null);
        var handler = new DeleteNoteHandler(wrongUserRepo);
        var wrongUserCommand = new DeleteNoteCommandRequest(note.Id, UserId.New());

        var notFoundResult = await handler.Handle(wrongUserCommand);

        Assert.Null(notFoundResult);
        Assert.False(wrongUserRepo.WasDeleted);
        Console.WriteLine($"[3] 非擁有者刪除：回傳 null，未執行刪除");

        // --- 4. 擁有者刪除，觸發 NoteDeletedEvent ---
        var ownerRepo = new FakeNoteRepository(returnNote: note);
        handler = new DeleteNoteHandler(ownerRepo);
        var deleteCommand = new DeleteNoteCommandRequest(note.Id, userId);

        var result = await handler.Handle(deleteCommand);

        Assert.NotNull(result);
        Assert.True(ownerRepo.WasDeleted);
        Assert.Same(note, ownerRepo.DeletedNote);

        var deletedEvent = Assert.Single(note.DomainEvents.OfType<NoteDeletedEvent>());
        Assert.Equal(note.Id, deletedEvent.NoteId);
        Console.WriteLine($"[4] 擁有者刪除成功，NoteDeletedEvent 觸發（NoteId: {deletedEvent.NoteId.Value}）");
    }
}

file class FakeNoteRepository(Note? returnNote) : INoteRepository
{
    public bool WasDeleted { get; private set; }
    public Note? DeletedNote { get; private set; }

    public Task<Note?> GetByIdAsync(NoteId id, CancellationToken ct = default) =>
        Task.FromResult(returnNote);

    public Task<Note?> GetByIdAndUserIdAsync(NoteId id, UserId userId, CancellationToken ct = default) =>
        Task.FromResult(returnNote);

    public Task UpdateAsync(Note note, CancellationToken ct = default) => Task.CompletedTask;

    public Task DeleteAsync(Note note, CancellationToken ct = default)
    {
        WasDeleted = true;
        DeletedNote = note;
        return Task.CompletedTask;
    }
}
