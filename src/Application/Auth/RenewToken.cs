using CoreMesh.Dispatching.Abstractions;
using Domain.Shared;
using Domain.Users;

namespace Application.Auth;

public record RenewTokenCommandRequest(string RawRefreshToken) : IRequest<TokenResponse?>;

public class RenewTokenHandler(
    IUserRepository userRepository,
    ITokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork) : IRequestHandler<RenewTokenCommandRequest, TokenResponse?>
{
    public async Task<TokenResponse?> Handle(RenewTokenCommandRequest command, CancellationToken ct)
    {
        var hash = TokenHasher.Hash(command.RawRefreshToken);
        var existing = await userRepository.FindRefreshTokenByHashAsync(hash, ct);
        if (existing is null || !existing.IsValid) return null;

        existing.Revoke();

        var refreshData = tokenIssuer.GenerateRefreshToken();
        await userRepository.AddRefreshTokenAsync(
            Domain.Users.RefreshToken.Create(
                existing.UserId, refreshData.Hash,
                DateTime.UtcNow.AddDays(30)), ct);

        await unitOfWork.SaveChangesAsync(ct);

        var issued = tokenIssuer.IssueAccessToken(existing.UserId);
        return new TokenResponse(issued.Value, refreshData.Raw, issued.ExpiresIn);
    }
}
