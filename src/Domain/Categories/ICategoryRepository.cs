using Domain.Users;

namespace Domain.Categories;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken ct = default);
    Task<IReadOnlyList<CategorySummary>> GetAllByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task<Category?> GetByIdAndUserIdAsync(CategoryId id, UserId userId, CancellationToken ct = default);
    Task<bool> IsInUseAsync(CategoryId id, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task DeleteAsync(Category category, CancellationToken ct = default);
}

public record CategorySummary(Guid Id, string Name, int NoteCount);
