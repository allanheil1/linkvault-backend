using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Stats.Queries;

public record StatsOverviewQuery(Guid UserId) : IRequest<StatsOverviewDto>;
public record PopularTagsQuery(Guid UserId, int Limit = 5) : IRequest<IEnumerable<PopularTagDto>>;

public class StatsOverviewQueryHandler : IRequestHandler<StatsOverviewQuery, StatsOverviewDto>
{
    private readonly IAppDbContext _context;

    public StatsOverviewQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<StatsOverviewDto> Handle(StatsOverviewQuery request, CancellationToken cancellationToken)
    {
        var totalLinks = await _context.Links.CountAsync(l => l.UserId == request.UserId, cancellationToken);
        var favoriteLinks = await _context.Links.CountAsync(l => l.UserId == request.UserId && l.IsFavorite, cancellationToken);
        var totalTags = await _context.Tags.CountAsync(t => t.UserId == request.UserId, cancellationToken);
        var totalCollections = await _context.Collections.CountAsync(c => c.UserId == request.UserId, cancellationToken);

        return new StatsOverviewDto(totalLinks, favoriteLinks, totalTags, totalCollections);
    }
}

public class PopularTagsQueryHandler : IRequestHandler<PopularTagsQuery, IEnumerable<PopularTagDto>>
{
    private readonly IAppDbContext _context;

    public PopularTagsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PopularTagDto>> Handle(PopularTagsQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, 20);

        var data = await _context.LinkTags
            .AsNoTracking()
            .Where(lt => lt.Link.UserId == request.UserId)
            .GroupBy(lt => new { lt.TagId, lt.Tag.Name })
            .Select(g => new PopularTagDto(g.Key.TagId, g.Key.Name, g.Count()))
            .OrderByDescending(t => t.LinkCount)
            .ThenBy(t => t.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return data;
    }
}
