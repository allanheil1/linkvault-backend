using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Collections.Queries;

public record GetCollectionQuery(Guid UserId, Guid CollectionId) : IRequest<CollectionDto>;

public class GetCollectionQueryHandler : IRequestHandler<GetCollectionQuery, CollectionDto?>
{
    private readonly IAppDbContext _context;

    public GetCollectionQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CollectionDto?> Handle(GetCollectionQuery request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);

        return collection is null ? null : new CollectionDto(collection.Id, collection.Name, collection.CreatedAt);
    }
}
