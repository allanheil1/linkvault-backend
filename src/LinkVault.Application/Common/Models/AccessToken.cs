namespace LinkVault.Application.Common.Models;

public record AccessToken(string Token, DateTimeOffset ExpiresAt);
