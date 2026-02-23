using LinkVault.Application.Common.Interfaces;

namespace LinkVault.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string hash, string password) => BCrypt.Net.BCrypt.Verify(password, hash);
}
