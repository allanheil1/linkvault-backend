using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Tags.Commands;

public record CreateTagCommand(Guid UserId, string Name) : IRequest<TagDto>;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly IAppDbContext _context;

    public CreateTagCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Name.Trim().ToLowerInvariant();
        var exists = await _context.Tags.AnyAsync(t => t.UserId == request.UserId && t.Name.ToLower() == normalized, cancellationToken);
        if (exists)
            throw new InvalidOperationException("Tag already exists.");

        var tag = new LinkVault.Domain.Entities.Tag
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim()
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return new TagDto(tag.Id, tag.Name);
    }
}
