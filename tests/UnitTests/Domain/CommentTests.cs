using Domain.Comments;
using Domain.Comments.Events;
using Domain.Notes;
using Domain.Users;

namespace UnitTests.Domain;

public class CommentTests
{
    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_EmptyContent_ReturnsError()
    {
        var result = Comment.Create(NoteId.New(), UserId.New(), "   ");

        Assert.False(result.IsSuccess);
        Assert.Equal(Comment.Errors.EmptyContent.Code, result.Error.Code);
    }

    [Fact]
    public void Create_TopLevel_RaisesEventWithNullParentCommentId()
    {
        var result = Comment.Create(NoteId.New(), UserId.New(), "留言內容");

        Assert.True(result.IsSuccess);
        var ev = Assert.Single(result.Value.DomainEvents.OfType<CommentCreatedEvent>());
        Assert.Null(ev.ParentCommentId);
    }

    [Fact]
    public void Create_Nested_EventContainsParentCommentId()
    {
        var parentId = CommentId.New();

        var result = Comment.Create(NoteId.New(), UserId.New(), "回覆內容", parentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(parentId, result.Value.ParentCommentId);
        var ev = Assert.Single(result.Value.DomainEvents.OfType<CommentCreatedEvent>());
        Assert.Equal(parentId.Value, ev.ParentCommentId);
    }

    // ── UpdateContent ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateContent_EmptyContent_ReturnsError()
    {
        var comment = Comment.Create(NoteId.New(), UserId.New(), "原始內容").Value;
        var originalContent = comment.Content;

        var result = comment.UpdateContent("");

        Assert.False(result.IsSuccess);
        Assert.Equal(originalContent, comment.Content);
    }

    [Fact]
    public void UpdateContent_ValidContent_UpdatesAndRaisesEvent()
    {
        var comment = Comment.Create(NoteId.New(), UserId.New(), "原始內容").Value;
        comment.ClearDomainEvents();

        var result = comment.UpdateContent("更新後的內容");

        Assert.True(result.IsSuccess);
        Assert.Equal("更新後的內容", comment.Content);
        Assert.Single(comment.DomainEvents.OfType<CommentEditedEvent>());
    }

    // ── Like ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Like_ReturnsCommentLikeWithCorrectIds()
    {
        var commentId = CommentId.New();
        var userId = UserId.New();
        var noteId = NoteId.New();
        var comment = Comment.Create(noteId, userId, "內容").Value;

        var like = comment.Like(userId);

        Assert.Equal(comment.Id, like.CommentId);
        Assert.Equal(userId, like.UserId);
    }

    [Fact]
    public void Like_RaisesCommentLikedEvent()
    {
        var comment = Comment.Create(NoteId.New(), UserId.New(), "內容").Value;
        comment.ClearDomainEvents();
        var likerId = UserId.New();

        comment.Like(likerId);

        var ev = Assert.Single(comment.DomainEvents.OfType<CommentLikedEvent>());
        Assert.Equal(likerId.Value, ev.UserId);
    }

    // ── Unlike ───────────────────────────────────────────────────────────────

    [Fact]
    public void Unlike_RaisesCommentUnlikedEvent()
    {
        var comment = Comment.Create(NoteId.New(), UserId.New(), "內容").Value;
        comment.ClearDomainEvents();

        comment.Unlike();

        Assert.Single(comment.DomainEvents.OfType<CommentUnlikedEvent>());
    }
}
