using Domain.Shared;

namespace Domain.Users;

public sealed class UserIdentityId : ValueObject
{
    public Guid Value { get; }

    public UserIdentityId(Guid value) => Value = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
