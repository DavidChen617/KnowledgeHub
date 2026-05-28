using Domain.Shared;

namespace Domain.Users;

public sealed class RefreshTokenId : ValueObject
{
    public Guid Value { get; }

    public RefreshTokenId(Guid value) => Value = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
