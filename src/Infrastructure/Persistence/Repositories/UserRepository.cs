using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<UserId> ids, CancellationToken ct = default)
    {
        var list = ids.ToList();
        return await db.Users.Where(u => list.Contains(u.Id)).ToListAsync(ct);
    }

    public Task<UserIdentity?> FindIdentityAsync(string provider, string providerSub, CancellationToken ct = default) =>
        db.UserIdentities.FirstOrDefaultAsync(
            i => i.Provider == provider && i.ProviderSub == providerSub, ct);

    public Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);

    public async Task AddIdentityAsync(UserIdentity identity, CancellationToken ct = default) =>
        await db.UserIdentities.AddAsync(identity, ct);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default) => Task.CompletedTask;
}
