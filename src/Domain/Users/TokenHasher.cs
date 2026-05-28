using System.Security.Cryptography;
using System.Text;

namespace Domain.Users;

public static class TokenHasher
{
    public static string Hash(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
