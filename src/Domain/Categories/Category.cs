using Domain.Shared;
using Domain.Users;

namespace Domain.Categories;

public class Category : AggregateRoot<CategoryId>
{
    public UserId UserId { get; }
    public string Name { get; private set; }

    private Category(CategoryId id, UserId userId, string name) : base(id)
    {
        UserId = userId;
        Name = name;
    }

    public static Category Create(UserId userId, string name) =>
        new(CategoryId.New(), userId, name);
}
