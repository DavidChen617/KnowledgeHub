using Domain.Notes;
using Domain.Users;
using Infrastructure.Persistence.Repositories;

namespace IntegrationTests.Repositories;

public class NoteRepositoryTests
{
    private static TestDbContext CreateDb() => TestDbContext.Create();

    [Fact]
    public async Task Given_NoteAdded_When_GetByIdAsync_Then_ReturnsNote()
    {
        await using var db = CreateDb();
        var repo = new NoteRepository(db);
        var userId = UserId.New();
        var note = Note.Create(userId, "測試標題", "測試內容").Value;

        await repo.AddAsync(note);
        await db.SaveChangesAsync();

        var found = await repo.GetByIdAsync(note.Id);

        Assert.NotNull(found);
        Assert.Equal(note.Id, found.Id);
        Assert.Equal("測試標題", found.Title);
    }

    [Fact]
    public async Task Given_NoteUpdated_When_GetByIdAsync_Then_ReturnsUpdatedContent()
    {
        await using var db = CreateDb();
        var repo = new NoteRepository(db);
        var userId = UserId.New();
        var note = Note.Create(userId, "原始標題", "原始內容").Value;
        await repo.AddAsync(note);
        await db.SaveChangesAsync();

        note.UpdateTitle("更新標題");
        await repo.Update(note);
        await db.SaveChangesAsync();

        var found = await repo.GetByIdAsync(note.Id);

        Assert.Equal("更新標題", found!.Title);
    }

    [Fact]
    public async Task Given_NoteDeleted_When_GetByIdAsync_Then_ReturnsNull()
    {
        await using var db = CreateDb();
        var repo = new NoteRepository(db);
        var note = Note.Create(UserId.New(), "標題", "內容").Value;
        await repo.AddAsync(note);
        await db.SaveChangesAsync();

        await repo.DeleteAsync(note);
        await db.SaveChangesAsync();

        var found = await repo.GetByIdAsync(note.Id);

        Assert.Null(found);
    }

    [Fact]
    public async Task Given_NoteWithSharedToken_When_GetBySharedTokenAsync_Then_ReturnsNote()
    {
        await using var db = CreateDb();
        var repo = new NoteRepository(db);
        var note = Note.Create(UserId.New(), "標題", "內容").Value;
        var token = note.CreateSharedLink();
        await repo.AddAsync(note);
        await db.SaveChangesAsync();

        var found = await repo.GetBySharedTokenAsync(token);

        Assert.NotNull(found);
        Assert.Equal(note.Id, found.Id);
    }

    [Fact]
    public async Task Given_MultipleNotes_When_GetAllByUserIdAsync_Then_ReturnsOnlyUserNotes()
    {
        await using var db = CreateDb();
        var repo = new NoteRepository(db);
        var userId = UserId.New();
        var otherUserId = UserId.New();

        var note1 = Note.Create(userId, "筆記1", "").Value;
        var note2 = Note.Create(userId, "筆記2", "").Value;
        var other = Note.Create(otherUserId, "其他人的筆記", "").Value;

        await repo.AddAsync(note1);
        await repo.AddAsync(note2);
        await repo.AddAsync(other);
        await db.SaveChangesAsync();

        var results = await repo.GetAllByUserIdAsync(userId);

        Assert.Equal(2, results.Count);
        Assert.All(results, n => Assert.Equal(userId, n.UserId));
    }
}
