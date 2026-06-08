using Domain.Categories;
using Domain.Users;
using Infrastructure.Persistence.Repositories;

namespace IntegrationTests.Repositories;

public class CategoryRepositoryTests
{
    private static TestDbContext CreateDb() => TestDbContext.Create();

    [Fact]
    public async Task Given_CategoryAdded_When_GetAllByUserIdAsync_Then_AppearsInList()
    {
        await using var db = CreateDb();
        var repo = new CategoryRepository(db);
        var userId = UserId.New();
        var category = Category.Create(userId, "測試分類").Value;

        await repo.AddAsync(category);
        await db.SaveChangesAsync();

        var results = await repo.GetAllByUserIdAsync(userId);

        Assert.Single(results);
        Assert.Equal("測試分類", results[0].Name);
    }

    [Fact]
    public async Task Given_CategoryDeleted_When_GetAllByUserIdAsync_Then_NotInList()
    {
        await using var db = CreateDb();
        var repo = new CategoryRepository(db);
        var userId = UserId.New();
        var category = Category.Create(userId, "要刪除的分類").Value;
        await repo.AddAsync(category);
        await db.SaveChangesAsync();

        var entity = await db.Categories.FindAsync(category.Id);
        await repo.DeleteAsync(entity!);
        await db.SaveChangesAsync();

        var results = await repo.GetAllByUserIdAsync(userId);

        Assert.Empty(results);
    }
}
