using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Auth.Queries;

public record MeQuery(Guid UserId) : IRequest<AuthUserDto>;

public class MeQueryHandler : IRequestHandler<MeQuery, AuthUserDto?>
{
    private readonly IAppDbContext _context;

    public MeQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<AuthUserDto?> Handle(MeQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        return user == null ? null : new AuthUserDto(user.Id, user.Name, user.Email);
    }
}
