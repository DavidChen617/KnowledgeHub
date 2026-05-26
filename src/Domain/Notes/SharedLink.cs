using Domain.Shared;

namespace Domain.Notes;

public class SharedLink : ValueObject
{
    public string Token { get; }

    private SharedLink(string token) => Token = token;

    public static SharedLink Create() =>
        new(GenerateToken());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Token;
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
}
