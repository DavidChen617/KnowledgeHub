using Domain.Notes;

namespace UnitTests.Domain;

public class NoteContentTests
{
    // ── ParseLinks ──────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidUuid_When_ParseLinks_Then_ReturnsNoteId()
    {
        var id = Guid.NewGuid();
        var content = new NoteContent($"參考 [[{id}]]");

        Assert.Single(content.LinkedNoteIds);
        Assert.Equal(id, content.LinkedNoteIds[0].Value);
    }

    [Fact]
    public void Given_InvalidFormat_When_ParseLinks_Then_ReturnsEmpty()
    {
        var content = new NoteContent("[[not-a-uuid]] [[123]]");

        Assert.Empty(content.LinkedNoteIds);
    }

    [Fact]
    public void Given_DuplicateUuid_When_ParseLinks_Then_Deduplicated()
    {
        var id = Guid.NewGuid();
        var content = new NoteContent($"[[{id}]] [[{id}]]");

        Assert.Single(content.LinkedNoteIds);
    }

    [Fact]
    public void Given_EmptyContent_When_ParseLinks_Then_ReturnsEmpty()
    {
        var content = new NoteContent(string.Empty);

        Assert.Empty(content.LinkedNoteIds);
    }

    [Fact]
    public void Given_MultipleLinks_When_ParseLinks_Then_AllParsed()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var content = new NoteContent($"[[{id1}]] 中間文字 [[{id2}]]");

        Assert.Equal(2, content.LinkedNoteIds.Count);
    }

    // ── ParseImages ─────────────────────────────────────────────────────────

    [Fact]
    public void Given_SingleImage_When_ParseImages_Then_ReturnsUrl()
    {
        var content = new NoteContent("![圖](https://example.com/a.png)");

        Assert.Single(content.ImageUrls);
        Assert.Equal("https://example.com/a.png", content.ImageUrls[0]);
    }

    [Fact]
    public void Given_DuplicateUrl_When_ParseImages_Then_Deduplicated()
    {
        var content = new NoteContent("![a](https://x.com/img.png) ![b](https://x.com/img.png)");

        Assert.Single(content.ImageUrls);
    }

    [Fact]
    public void Given_NoImages_When_ParseImages_Then_ReturnsEmpty()
    {
        var content = new NoteContent("純文字，沒有圖片");

        Assert.Empty(content.ImageUrls);
    }

    [Fact]
    public void Given_MultipleImages_When_ParseImages_Then_AllParsed()
    {
        var content = new NoteContent("![a](https://x.com/1.png)\n![b](https://x.com/2.png)");

        Assert.Equal(2, content.ImageUrls.Count);
    }

    // ── GetSurroundingContext ────────────────────────────────────────────────

    [Fact]
    public void Given_ImageInMiddle_When_GetSurroundingContext_Then_ReturnsContextWithPlaceholder()
    {
        var url = "https://x.com/img.png";
        var lines = new[]
        {
            "line1", "line2", "line3",
            $"![alt]({url})",
            "line5", "line6", "line7"
        };
        var content = new NoteContent(string.Join('\n', lines));

        var ctx = content.GetSurroundingContext(url);

        Assert.Contains("[image]", ctx);
        Assert.Contains("line2", ctx);
        Assert.Contains("line6", ctx);
        Assert.DoesNotContain($"![alt]({url})", ctx);
    }

    [Fact]
    public void Given_ImageAtStart_When_GetSurroundingContext_Then_NoOutOfBounds()
    {
        var url = "https://x.com/img.png";
        var content = new NoteContent($"![alt]({url})\nline2\nline3");

        var ctx = content.GetSurroundingContext(url);

        Assert.Contains("[image]", ctx);
    }

    [Fact]
    public void Given_UrlNotInContent_When_GetSurroundingContext_Then_ReturnsEmpty()
    {
        var content = new NoteContent("沒有圖片的內容");

        var ctx = content.GetSurroundingContext("https://x.com/nonexistent.png");

        Assert.Equal(string.Empty, ctx);
    }

    // ── ReplaceImageWithDescription ──────────────────────────────────────────

    [Fact]
    public void Given_MatchingUrl_When_ReplaceImageWithDescription_Then_ImageReplaced()
    {
        var url = "https://x.com/img.png";
        var original = $"文字 ![alt]({url}) 更多文字";

        var result = NoteContent.ReplaceImageWithDescription(original, url, "系統架構圖");

        Assert.Contains("[圖片描述: 系統架構圖]", result);
        Assert.DoesNotContain($"![alt]({url})", result);
    }

    [Fact]
    public void Given_NonMatchingUrl_When_ReplaceImageWithDescription_Then_Unchanged()
    {
        var target = "https://x.com/target.png";
        var other = "https://x.com/other.png";
        var original = $"![alt]({other})";

        var result = NoteContent.ReplaceImageWithDescription(original, target, "描述");

        Assert.Equal(original, result);
    }

    [Fact]
    public void Given_MultipleImages_When_ReplaceImageWithDescription_Then_OnlyTargetReplaced()
    {
        var target = "https://x.com/target.png";
        var other = "https://x.com/other.png";
        var original = $"![a]({target}) ![b]({other})";

        var result = NoteContent.ReplaceImageWithDescription(original, target, "描述");

        Assert.Contains("[圖片描述: 描述]", result);
        Assert.Contains($"![b]({other})", result);
    }
}
