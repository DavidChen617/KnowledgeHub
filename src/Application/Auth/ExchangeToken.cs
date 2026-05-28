using CoreMesh.Dispatching.Abstractions;
using Domain.Shared;
using Domain.Users;

namespace Application.Auth;

public record ExchangeTokenCommandRequest(string ExternalToken) : IRequest<TokenResponse?>;

public record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public class ExchangeTokenHandler(
    IIdentityProvider identityProvider,
    IUserRepository userRepository,
    ITokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork) : IRequestHandler<ExchangeTokenCommandRequest, TokenResponse?>
{
    public async Task<TokenResponse?> Handle(ExchangeTokenCommandRequest command, CancellationToken ct)
    {
        var identity = await identityProvider.ValidateAsync(command.ExternalToken, ct);
        if (identity is null) return null;

        var userIdentity = await userRepository.FindIdentityAsync(
            identityProvider.ProviderName, identity.Sub, ct);

        User user;
        if (userIdentity is null)
        {
            user = User.Create(identity.Email, identity.Name, identity.AvatarUrl);
            await userRepository.AddAsync(user, ct);
            await userRepository.AddIdentityAsync(
                UserIdentity.Create(user.Id, identityProvider.ProviderName, identity.Sub), ct);
        }
        else
        {
            user = (await userRepository.GetByIdAsync(userIdentity.UserId, ct))!;
            user.UpdateAvatar(identity.AvatarUrl);
        }

        var refreshData = tokenIssuer.GenerateRefreshToken();
        await userRepository.AddRefreshTokenAsync(
            Domain.Users.RefreshToken.Create(
                user.Id, refreshData.Hash,
                DateTime.UtcNow.AddDays(30)), ct);

        await unitOfWork.SaveChangesAsync(ct);

        var issued = tokenIssuer.IssueAccessToken(user.Id);
        return new TokenResponse(issued.Value, refreshData.Raw, issued.ExpiresIn);
    }
}
