using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Collections.Commands;

public record DeleteCollectionCommand(Guid UserId, Guid CollectionId) : IRequest<Unit>;

public class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand, Unit>
{
    private readonly IAppDbContext _context;

    public DeleteCollectionCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _context.Collections.FirstOrDefaultAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);
        if (collection == null)
            throw new KeyNotFoundException("Collection not found.");

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
