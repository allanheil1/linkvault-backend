using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Queries;

public record GetLinkQuery(Guid UserId, Guid LinkId) : IRequest<LinkDto>;

public class GetLinkQueryHandler : IRequestHandler<GetLinkQuery, LinkDto?>
{
    private readonly IAppDbContext _context;

    public GetLinkQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<LinkDto?> Handle(GetLinkQuery request, CancellationToken cancellationToken)
    {
        var link = await _context.Links
            .AsNoTracking()
            .Include(l => l.LinkTags)
            .FirstOrDefaultAsync(l => l.Id == request.LinkId && l.UserId == request.UserId, cancellationToken);

        return link?.ToDto();
    }
}
