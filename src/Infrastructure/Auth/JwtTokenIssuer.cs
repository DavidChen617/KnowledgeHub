using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

internal sealed class JwtTokenIssuer(IConfiguration config) : ITokenIssuer
{
    private readonly string _secret = config["Jwt:Secret"]!;
    private readonly int _expiresInMinutes = int.Parse(config["Jwt:ExpiresInMinutes"] ?? "60");

    public IssuedToken IssueAccessToken(UserId userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())],
            expires: DateTime.UtcNow.AddMinutes(_expiresInMinutes),
            signingCredentials: credentials);

        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), _expiresInMinutes * 60);
    }

    public RefreshTokenData GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = TokenHasher.Hash(raw);
        return new RefreshTokenData(raw, hash);
    }
}
