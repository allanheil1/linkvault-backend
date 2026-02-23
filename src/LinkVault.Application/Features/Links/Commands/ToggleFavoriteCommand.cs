using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Commands;

public record ToggleFavoriteCommand(Guid UserId, Guid LinkId) : IRequest<LinkDto>;

public class ToggleFavoriteCommandHandler : IRequestHandler<ToggleFavoriteCommand, LinkDto>
{
    private readonly IAppDbContext _context;
    private readonly IDateTimeProvider _clock;

    public ToggleFavoriteCommandHandler(IAppDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<LinkDto> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var link = await _context.Links
            .Include(l => l.LinkTags)
            .FirstOrDefaultAsync(l => l.Id == request.LinkId && l.UserId == request.UserId, cancellationToken);

        if (link == null)
            throw new KeyNotFoundException("Link not found");

        link.IsFavorite = !link.IsFavorite;
        link.UpdatedAt = _clock.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return link.ToDto();
    }
}
