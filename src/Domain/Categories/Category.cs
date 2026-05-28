using Domain.Shared;
using Domain.Users;
using ShareKernal;

namespace Domain.Categories;

public class Category : AggregateRoot<CategoryId>
{
    public static class Errors
    {
        public static readonly Error EmptyName = new("Category.EmptyName", "Name cannot be empty", ErrorType.Validation);
    }

    public UserId UserId { get; }
    public string Name { get; private set; }

    private Category(CategoryId id, UserId userId, string name) : base(id)
    {
        UserId = userId;
        Name = name;
    }

    public static Result<Category> Create(UserId userId, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Errors.EmptyName;
        return Result.Success(new Category(CategoryId.New(), userId, name));
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Errors.EmptyName;
        Name = name;
        return Result.Success();
    }
}
