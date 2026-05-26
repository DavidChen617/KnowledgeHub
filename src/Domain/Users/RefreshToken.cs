using Domain.Shared;

namespace Domain.Users;

public class RefreshToken : Entity<Guid>
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    private RefreshToken(Guid id, string token, DateTime expiresAt) : base(id)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken Create(string token, int validDays = 7) =>
        new(Guid.NewGuid(), token, DateTime.UtcNow.AddDays(validDays));
}
