using Domain.Shared;

namespace Domain.Categories;

public sealed class CategoryId : ValueObject
{
    public Guid Value { get; }

    public CategoryId(Guid value) => Value = value;

    public static CategoryId New() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
