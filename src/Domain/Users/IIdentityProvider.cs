namespace Domain.Users;

public interface IIdentityProvider
{
    string ProviderName { get; }
    Task<ExternalIdentity?> ValidateAsync(string token, CancellationToken ct = default);
}

public record ExternalIdentity(string Sub, string Email, string Name, string? AvatarUrl);
