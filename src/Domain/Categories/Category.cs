using Domain.Categories.Events;

using Domain.Users;
using ShareKernal;

namespace Domain.Categories;

public sealed class CategoryId : ValueObject
{
    public Guid Value { get; }
    public CategoryId(Guid value) => Value = value;
    public static CategoryId New() => new(Guid.NewGuid());
    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}

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
        var category = new Category(CategoryId.New(), userId, name);
        category.RaiseDomainEvent(new CategoryCreatedEvent(category.Id.Value, userId.Value));
        return Result.Success(category);
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Errors.EmptyName;
        Name = name;
        RaiseDomainEvent(new CategoryUpdatedEvent(Id.Value, UserId.Value));
        return Result.Success();
    }

    public void Delete()
    {
        RaiseDomainEvent(new CategoryDeletedEvent(Id.Value, UserId.Value));
    }
}
