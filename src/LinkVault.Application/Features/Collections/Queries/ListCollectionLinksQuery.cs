using LinkVault.Application.Common.Interfaces;
using LinkVault.Application.Features.Links;
using LinkVault.Application.Features.Links.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Collections.Queries;

public record ListCollectionLinksQuery(Guid UserId, Guid CollectionId, int Page, int PageSize) : IRequest<PagedResult<LinkDto>>;

public class ListCollectionLinksQueryHandler : IRequestHandler<ListCollectionLinksQuery, PagedResult<LinkDto>>
{
    private readonly IAppDbContext _context;

    public ListCollectionLinksQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LinkDto>> Handle(ListCollectionLinksQuery request, CancellationToken cancellationToken)
    {
        var ownsCollection = await _context.Collections.AnyAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);
        if (!ownsCollection) throw new KeyNotFoundException("Collection not found.");

        var query = _context.Links
            .AsNoTracking()
            .Include(l => l.LinkTags)
            .Where(l => l.UserId == request.UserId && l.CollectionId == request.CollectionId)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LinkDto>(items.Select(l => l.ToDto()).ToList(), page, pageSize, total);
    }
}
