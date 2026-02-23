using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Collections.Commands;

public record CreateCollectionCommand(Guid UserId, string Name) : IRequest<CollectionDto>;

public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, CollectionDto>
{
    private readonly IAppDbContext _context;
    private readonly IDateTimeProvider _clock;

    public CreateCollectionCommandHandler(IAppDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<CollectionDto> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Collections.AnyAsync(c => c.UserId == request.UserId && c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException("Collection already exists.");

        var collection = new LinkVault.Domain.Entities.Collection
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim(),
            CreatedAt = _clock.UtcNow
        };

        _context.Collections.Add(collection);
        await _context.SaveChangesAsync(cancellationToken);

        return new CollectionDto(collection.Id, collection.Name, collection.CreatedAt);
    }
}
