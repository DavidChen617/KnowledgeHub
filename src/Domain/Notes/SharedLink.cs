using Domain.Shared;

namespace Domain.Notes;

public class SharedLink : ValueObject
{
    public string Token { get; }
    public SharePermission Permission { get; }

    private SharedLink(string token, SharePermission permission)
    {
        Token = token;
        Permission = permission;
    }

    public static SharedLink Create(SharePermission permission) =>
        new(GenerateToken(), permission);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Token;
        yield return Permission;
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
}
