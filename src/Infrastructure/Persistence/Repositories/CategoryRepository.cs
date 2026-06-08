using Domain.Categories;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await db.Categories.AddAsync(category, ct);
    }

    public async Task<IReadOnlyList<CategorySummary>> GetAllByUserIdAsync(UserId userId, CancellationToken ct = default)
    {
        return await db.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CategorySummary(
                c.Id.Value,
                c.Name,
                db.Notes.Count(n => n.CategoryId == c.Id)))
            .ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAndUserIdAsync(CategoryId id, UserId userId, CancellationToken ct = default)
    {
        return await db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
    }

    public async Task<bool> IsInUseAsync(CategoryId id, CancellationToken ct = default)
    {
        return await db.Notes.AnyAsync(n => n.CategoryId == id, ct);
    }

    public Task Update(Category category, CancellationToken ct = default) => Task.CompletedTask;

    public Task DeleteAsync(Category category, CancellationToken ct = default)
    {
        db.Categories.Remove(category);
        return Task.CompletedTask;
    }
}
