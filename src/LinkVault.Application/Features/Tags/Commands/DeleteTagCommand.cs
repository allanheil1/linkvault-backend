using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Tags.Commands;

public record DeleteTagCommand(Guid UserId, Guid TagId) : IRequest<Unit>;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Unit>
{
    private readonly IAppDbContext _context;

    public DeleteTagCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == request.TagId && t.UserId == request.UserId, cancellationToken);
        if (tag == null)
            throw new KeyNotFoundException("Tag not found.");

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
