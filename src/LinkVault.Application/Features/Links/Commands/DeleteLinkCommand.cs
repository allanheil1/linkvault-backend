using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Commands;

public record DeleteLinkCommand(Guid UserId, Guid LinkId) : IRequest<Unit>;

public class DeleteLinkCommandHandler : IRequestHandler<DeleteLinkCommand, Unit>
{
    private readonly IAppDbContext _context;

    public DeleteLinkCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _context.Links.FirstOrDefaultAsync(l => l.Id == request.LinkId && l.UserId == request.UserId, cancellationToken);
        if (link == null)
            throw new KeyNotFoundException("Link not found");

        _context.Links.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
