using LinkVault.Application.Common.Interfaces;
using LinkVault.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Auth.Commands;

public record RefreshCommand(string RefreshToken) : IRequest<(RefreshResponse Response, RefreshTokenResult NewRefreshToken)>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, (RefreshResponse Response, RefreshTokenResult NewRefreshToken)>
{
    private readonly IAppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _clock;

    public RefreshCommandHandler(IAppDbContext context, ITokenService tokenService, IDateTimeProvider clock)
    {
        _context = context;
        _tokenService = tokenService;
        _clock = clock;
    }

    public async Task<(RefreshResponse Response, RefreshTokenResult NewRefreshToken)> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var hashed = TokenHashing.ComputeHash(request.RefreshToken);
        var existing = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashed, cancellationToken);

        if (existing == null || existing.RevokedAt != null || existing.ExpiresAt <= _clock.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        existing.RevokedAt = _clock.UtcNow;

        var newRefresh = _tokenService.CreateRefreshToken();
        var newRefreshEntity = new LinkVault.Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            TokenHash = newRefresh.TokenHash,
            ExpiresAt = newRefresh.ExpiresAt,
            CreatedAt = _clock.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshEntity);

        var accessToken = _tokenService.CreateAccessToken(existing.User);
        await _context.SaveChangesAsync(cancellationToken);

        return (new RefreshResponse(accessToken.Token), newRefresh);
    }
}
