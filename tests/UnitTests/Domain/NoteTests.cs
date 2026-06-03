using Domain.Notes;
using Domain.Notes.Events;
using Domain.Users;

namespace UnitTests.Domain;

public class NoteTests
{
    // ── ChunkByHeadings ──────────────────────────────────────────────────────

    [Fact]
    public void ChunkByHeadings_NoHeadings_SingleChunk()
    {
        var note = Note.Create(UserId.New(), "標題", "這是一段沒有 ### 標題的內容\n第二行\n第三行").Value;

        var structure = note.AddStructure("prompt", "這是一段沒有 ### 標題的內容\n第二行\n第三行", "描述");

        Assert.Single(structure.Chunks);
    }

    [Fact]
    public void ChunkByHeadings_MultipleHeadings_CorrectChunkCount()
    {
        var content = """
            ### 第一節
            第一節內容

            ### 第二節
            第二節內容

            ### 第三節
            第三節內容
            """;
        var note = Note.Create(UserId.New(), "標題", content).Value;

        var structure = note.AddStructure("prompt", content, "描述");

        Assert.Equal(3, structure.Chunks.Count);
    }

    [Fact]
    public void ChunkByHeadings_EmptyContent_NoChunks()
    {
        var note = Note.Create(UserId.New(), "標題").Value;

        var structure = note.AddStructure("prompt", string.Empty, "描述");

        Assert.Empty(structure.Chunks);
    }

    // ── SyncImages ──────────────────────────────────────────────────────────

    [Fact]
    public void SyncImages_NewImage_AddedWithEnableTrue()
    {
        var note = Note.Create(UserId.New(), "標題").Value;
        var url = "https://res.cloudinary.com/test/image/upload/v1/new.png";

        note.UpdateContent($"![圖]({url})");

        Assert.Single(note.Images);
        Assert.True(note.Images[0].Enable);
    }

    [Fact]
    public void SyncImages_ImageRemoved_DisabledAndEventRaised()
    {
        var url = "https://res.cloudinary.com/test/image/upload/v1/img.png";
        var note = Note.Create(UserId.New(), "標題", $"![圖]({url})").Value;
        note.ClearDomainEvents();

        note.UpdateContent("移除圖片後的內容");

        var disabledImg = note.Images.Single(img => img.PublicUrl == url);
        Assert.False(disabledImg.Enable);
        Assert.Single(note.DomainEvents.OfType<NoteImagesChangedEvent>());
    }

    [Fact]
    public void SyncImages_ImageUnchanged_NoImagesChangedEvent()
    {
        var url = "https://res.cloudinary.com/test/image/upload/v1/img.png";
        var note = Note.Create(UserId.New(), "標題", $"![圖]({url})").Value;
        note.ClearDomainEvents();

        note.UpdateContent($"更新文字，保留圖片 ![圖]({url})");

        Assert.DoesNotContain(note.DomainEvents, e => e is NoteImagesChangedEvent);
    }

    // ── SharedLink ──────────────────────────────────────────────────────────

    [Fact]
    public void CreateSharedLink_TokenIsUrlSafe()
    {
        var note = Note.Create(UserId.New(), "標題").Value;

        var token = note.CreateSharedLink();

        Assert.NotNull(token);
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
        Assert.Equal(note.SharedLinkToken, token);
    }

    [Fact]
    public void CreateSharedLink_RaisesSharedLinkCreatedEvent()
    {
        var note = Note.Create(UserId.New(), "標題").Value;
        note.ClearDomainEvents();

        note.CreateSharedLink();

        Assert.Single(note.DomainEvents.OfType<SharedLinkCreatedEvent>());
    }

    [Fact]
    public void DeleteSharedLink_ClearsTokenAndRaisesEvent()
    {
        var note = Note.Create(UserId.New(), "標題").Value;
        note.CreateSharedLink();
        note.ClearDomainEvents();

        note.DeleteSharedLink();

        Assert.Null(note.SharedLinkToken);
        Assert.Single(note.DomainEvents.OfType<SharedLinkDeletedEvent>());
    }

    [Fact]
    public void DeleteSharedLink_WhenNoToken_DoesNotRaiseEvent()
    {
        var note = Note.Create(UserId.New(), "標題").Value;
        note.ClearDomainEvents();

        note.DeleteSharedLink();

        Assert.DoesNotContain(note.DomainEvents, e => e is SharedLinkDeletedEvent);
    }
}
