using System.Security.Cryptography;
using System.Text;

namespace LinkVault.Application.Features.Auth;

public static class TokenHashing
{
    public static string ComputeHash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
