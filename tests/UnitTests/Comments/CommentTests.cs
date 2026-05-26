using Domain.Comments;
using Domain.Notes;
using Domain.Users;

namespace UnitTests.Comments;

public class CommentTests
{
    private static readonly NoteId AnyNote = NoteId.New();
    private static readonly UserId AnyUser = UserId.New();

    [Fact]
    public void Create_WithValidContent_Succeeds()
    {
        var comment = Comment.Create(AnyNote, AnyUser, "這是一個留言");
        Assert.Equal("這是一個留言", comment.Content);
    }

    [Fact]
    public void Create_WithEmptyContent_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Comment.Create(AnyNote, AnyUser, string.Empty));
    }

    [Fact]
    public void Create_WithWhitespaceOnly_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Comment.Create(AnyNote, AnyUser, "   "));
    }
}
