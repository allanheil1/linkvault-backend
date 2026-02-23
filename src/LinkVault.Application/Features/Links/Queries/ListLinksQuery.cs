using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Queries;

public record ListLinksQuery(
    Guid UserId,
    string? Search,
    Guid? TagId,
    Guid? CollectionId,
    bool? Favorite,
    int Page,
    int PageSize,
    string? Sort) : IRequest<PagedResult<LinkDto>>;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);

public class ListLinksQueryHandler : IRequestHandler<ListLinksQuery, PagedResult<LinkDto>>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdat", "-createdat", "title", "-title"
    };

    private readonly IAppDbContext _context;

    public ListLinksQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LinkDto>> Handle(ListLinksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Links
            .AsNoTracking()
            .Include(l => l.LinkTags)
            .Where(l => l.UserId == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim().ToLowerInvariant()}%";
            query = query.Where(l =>
                EF.Functions.Like(l.Title.ToLower(), term) ||
                EF.Functions.Like((l.Note ?? string.Empty).ToLower(), term) ||
                EF.Functions.Like(l.Url.ToLower(), term));
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(l => l.LinkTags.Any(lt => lt.TagId == request.TagId));
        }

        if (request.CollectionId.HasValue)
        {
            query = query.Where(l => l.CollectionId == request.CollectionId);
        }

        if (request.Favorite.HasValue)
        {
            query = query.Where(l => l.IsFavorite == request.Favorite.Value);
        }

        var sort = AllowedSort.Contains(request.Sort ?? string.Empty) ? request.Sort!.ToLowerInvariant() : "-createdat";
        query = sort switch
        {
            "createdat" => query.OrderBy(l => l.CreatedAt),
            "-createdat" => query.OrderByDescending(l => l.CreatedAt),
            "title" => query.OrderBy(l => l.Title),
            "-title" => query.OrderByDescending(l => l.Title),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };

        var total = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LinkDto>(items.Select(l => l.ToDto()).ToList(), page, pageSize, total);
    }
}
