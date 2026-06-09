namespace Application.Interfaces;

public interface IStructureRateLimiter
{
    Task<bool> IsAllowedAsync(Guid userId, CancellationToken ct = default);
}
