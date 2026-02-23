namespace LinkVault.Application.Features.Auth;

public record AuthUserDto(Guid Id, string Name, string Email);

public record LoginResponse(string AccessToken, AuthUserDto User);

public record RefreshResponse(string AccessToken);

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
