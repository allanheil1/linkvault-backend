using LinkVault.Application.Common.Interfaces;
using LinkVault.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<(LoginResponse Response, RefreshTokenResult RefreshToken)>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, (LoginResponse Response, RefreshTokenResult RefreshToken)>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _clock;

    public LoginCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService, IDateTimeProvider clock)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _clock = clock;
    }

    public async Task<(LoginResponse Response, RefreshTokenResult RefreshToken)> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailLower, cancellationToken);

        if (user == null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        var refreshEntity = new LinkVault.Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshToken.TokenHash,
            ExpiresAt = refreshToken.ExpiresAt,
            CreatedAt = _clock.UtcNow
        };

        _context.RefreshTokens.Add(refreshEntity);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(accessToken.Token, new AuthUserDto(user.Id, user.Name, user.Email));
        return (response, refreshToken);
    }
}
