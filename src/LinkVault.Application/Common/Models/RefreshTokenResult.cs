namespace LinkVault.Application.Common.Models;

public record RefreshTokenResult(string Token, string TokenHash, DateTimeOffset ExpiresAt);
