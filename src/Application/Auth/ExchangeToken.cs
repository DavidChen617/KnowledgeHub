using CoreMesh.Dispatching.Abstractions;
using Domain.Notes;
using Domain.Shared;
using Domain.Users;
using ShareKernal;
using static Application.Auth.AuthErrors;
using static ShareKernal.Result;

namespace Application.Auth;

public record ExchangeTokenCommandRequest(string ExternalToken, string BaseUrl) : IRequest<Result<TokenResponse>>;

public record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public class ExchangeTokenHandler(
    IIdentityProvider identityProvider,
    IUserRepository userRepository,
    IImageStorage imageStorage,
    ITokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork) : IRequestHandler<ExchangeTokenCommandRequest, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(ExchangeTokenCommandRequest command, CancellationToken ct)
    {
        var identity = await identityProvider.ValidateAsync(command.ExternalToken, ct);
        if (identity is null) return InvalidToken;

        var userIdentity = await userRepository.FindIdentityAsync(
            identityProvider.ProviderName, identity.Sub, ct);

        User user;
        if (userIdentity is null)
        {
            var avatarUrl = await ResolveAvatarUrlAsync(identity.AvatarUrl, command.BaseUrl, ct);
            user = User.Create(identity.Email, identity.Name, avatarUrl);
            await userRepository.AddAsync(user, ct);
            await userRepository.AddIdentityAsync(
                UserIdentity.Create(user.Id, identityProvider.ProviderName, identity.Sub), ct);
        }
        else
        {
            user = (await userRepository.GetByIdAsync(userIdentity.UserId, ct))!;
        }

        var refreshData = tokenIssuer.GenerateRefreshToken();
        await userRepository.AddRefreshTokenAsync(
            Domain.Users.RefreshToken.Create(
                user.Id, refreshData.Hash,
                DateTime.UtcNow.AddDays(30)), ct);

        await unitOfWork.SaveChangesAsync(ct);

        var issued = tokenIssuer.IssueAccessToken(user.Id);
        return Success(new TokenResponse(issued.Value, refreshData.Raw, issued.ExpiresIn));
    }

    private async Task<string?> ResolveAvatarUrlAsync(string? externalUrl, string baseUrl, CancellationToken ct)
    {
        if (externalUrl is null) return null;
        var storageKey = await imageStorage.UploadFromUrlAsync(externalUrl, ct);
        return $"{baseUrl}/image/{storageKey}";
    }
}
