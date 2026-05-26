using Domain.Shared;

namespace Domain.Users;

public sealed class UserId : ValueObject
{
    public Guid Value { get; }

    public UserId(Guid value) => Value = value;

    public static UserId New() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
