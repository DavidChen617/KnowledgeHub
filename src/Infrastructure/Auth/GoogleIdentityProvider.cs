using Domain.Users;
using Google.Apis.Auth;

namespace Infrastructure.Auth;

internal sealed class GoogleIdentityProvider : IIdentityProvider
{
    public string ProviderName => "google";

    public async Task<ExternalIdentity?> ValidateAsync(string token, CancellationToken ct)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token);
            return new ExternalIdentity(payload.Subject, payload.Email, payload.Name, payload.Picture);
        }
        catch
        {
            return null;
        }
    }
}
