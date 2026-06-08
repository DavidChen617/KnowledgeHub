using Domain.Comments;
using Domain.Notes;
using Domain.Users;
using Infrastructure.Persistence.Repositories;

namespace IntegrationTests.Repositories;

public class CommentRepositoryTests
{
    private static TestDbContext CreateDb() => TestDbContext.Create();

    [Fact(Skip = "EF Core InMemory does not support GroupBy + ToDictionaryAsync; verified on real Postgres")]
    public async Task Given_TwoUsersLiked_When_GetLikeCountsAsync_Then_CountIsTwo()
    {
        await using var db = CreateDb();
        var repo = new CommentRepository(db);
        var noteId = NoteId.New();
        var comment = Comment.Create(noteId, UserId.New(), "留言內容").Value;
        await db.Comments.AddAsync(comment);
        await db.SaveChangesAsync();

        var like1 = CommentLike.Create(comment.Id, UserId.New());
        var like2 = CommentLike.Create(comment.Id, UserId.New());
        await repo.AddLikeAsync(like1);
        await repo.AddLikeAsync(like2);
        await db.SaveChangesAsync();

        var counts = await repo.GetLikeCountsAsync([comment.Id]);

        Assert.Equal(2, counts[comment.Id]);
    }

    [Fact]
    public async Task Given_UserALiked_When_GetLikedByUserAsync_Then_OnlyUserACommentReturned()
    {
        await using var db = CreateDb();
        var repo = new CommentRepository(db);
        var noteId = NoteId.New();
        var userA = UserId.New();
        var userB = UserId.New();

        var comment1 = Comment.Create(noteId, UserId.New(), "留言1").Value;
        var comment2 = Comment.Create(noteId, UserId.New(), "留言2").Value;
        await db.Comments.AddRangeAsync(comment1, comment2);
        await db.SaveChangesAsync();

        await repo.AddLikeAsync(CommentLike.Create(comment1.Id, userA));
        await repo.AddLikeAsync(CommentLike.Create(comment2.Id, userB));
        await db.SaveChangesAsync();

        var liked = await repo.GetLikedByUserAsync([comment1.Id, comment2.Id], userA);

        Assert.Single(liked);
        Assert.Contains(comment1.Id, liked);
        Assert.DoesNotContain(comment2.Id, liked);
    }
}
