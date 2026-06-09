using ShareKernal;


namespace Domain.Users;

public sealed class RefreshTokenId : ValueObject
{
    public Guid Value { get; }
    public RefreshTokenId(Guid value) => Value = value;
    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}

public class RefreshToken : Entity<RefreshTokenId>
{
    public UserId UserId { get; }
    public string TokenHash { get; }
    public DateTime ExpiresAt { get; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsValid => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

    private RefreshToken(RefreshTokenId id, UserId userId, string tokenHash, DateTime expiresAt) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken Create(UserId userId, string tokenHash, DateTime expiresAt) =>
        new(new RefreshTokenId(Guid.NewGuid()), userId, tokenHash, expiresAt);

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
