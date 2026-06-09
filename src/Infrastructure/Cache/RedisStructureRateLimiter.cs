using Application.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Cache;

internal sealed class RedisStructureRateLimiter(IDatabase db) : IStructureRateLimiter
{
    private const int Limit = 5;
    private const int WindowSeconds = 3600;

    public async Task<bool> IsAllowedAsync(Guid userId, CancellationToken ct = default)
    {
        var key = $"rate_limit:structure:{userId}";
        var count = await db.StringIncrementAsync(key);
        if (count == 1)
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(WindowSeconds));
        return count <= Limit;
    }
}
