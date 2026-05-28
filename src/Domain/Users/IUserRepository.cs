namespace Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<UserIdentity?> FindIdentityAsync(string provider, string providerSub, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task AddIdentityAsync(UserIdentity identity, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);
}
