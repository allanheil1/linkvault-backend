using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Collections.Commands;

public record UpdateCollectionCommand(Guid UserId, Guid CollectionId, string Name) : IRequest<CollectionDto>;

public class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, CollectionDto>
{
    private readonly IAppDbContext _context;

    public UpdateCollectionCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CollectionDto> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections.FirstOrDefaultAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);
        if (collection == null)
            throw new KeyNotFoundException("Collection not found.");

        var exists = await _context.Collections.AnyAsync(c => c.UserId == request.UserId && c.Id != request.CollectionId && c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException("Collection already exists.");

        collection.Name = request.Name.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return new CollectionDto(collection.Id, collection.Name, collection.CreatedAt);
    }
}
