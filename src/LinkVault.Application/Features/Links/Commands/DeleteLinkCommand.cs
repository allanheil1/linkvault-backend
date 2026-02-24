using LinkVault.Application.Common.Interfaces;
using LinkVault.Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Commands;

public record DeleteLinkCommand(Guid UserId, Guid LinkId) : IRequest<Result>;

public class DeleteLinkCommandHandler : IRequestHandler<DeleteLinkCommand, Result>
{
    private readonly IAppDbContext _context;

    public DeleteLinkCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _context.Links.FirstOrDefaultAsync(l => l.Id == request.LinkId && l.UserId == request.UserId, cancellationToken);
        if (link == null)
        {
            return Result.Failure(new ResultError(ResultErrorType.NotFound, "link_not_found", "Link not found."));
        }

        _context.Links.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
