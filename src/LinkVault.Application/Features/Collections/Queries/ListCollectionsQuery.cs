using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Collections.Queries;

public record ListCollectionsQuery(Guid UserId) : IRequest<IEnumerable<CollectionDto>>;

public class ListCollectionsQueryHandler : IRequestHandler<ListCollectionsQuery, IEnumerable<CollectionDto>>
{
    private readonly IAppDbContext _context;

    public ListCollectionsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CollectionDto>> Handle(ListCollectionsQuery request, CancellationToken cancellationToken)
    {
        var collections = await _context.Collections.AsNoTracking()
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return collections.Select(c => new CollectionDto(c.Id, c.Name, c.CreatedAt));
    }
}
