using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Tags.Commands;

public record UpdateTagCommand(Guid UserId, Guid TagId, string Name) : IRequest<TagDto>;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, TagDto>
{
    private readonly IAppDbContext _context;

    public UpdateTagCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TagDto> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == request.TagId && t.UserId == request.UserId, cancellationToken);
        if (tag == null)
            throw new KeyNotFoundException("Tag not found.");

        var normalized = request.Name.Trim().ToLowerInvariant();
        var exists = await _context.Tags.AnyAsync(t => t.UserId == request.UserId && t.Id != request.TagId && t.Name.ToLower() == normalized, cancellationToken);
        if (exists)
            throw new InvalidOperationException("Tag already exists.");

        tag.Name = request.Name.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return new TagDto(tag.Id, tag.Name);
    }
}
