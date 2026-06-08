namespace Domain.Users;

public interface ITokenIssuer
{
    IssuedToken IssueAccessToken(UserId userId);
    RefreshTokenData GenerateRefreshToken();
}

public record IssuedToken(string Value, int ExpiresIn);
public record RefreshTokenData(string Raw, string Hash);
