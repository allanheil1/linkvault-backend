using LinkVault.Application.Common.Interfaces;
using LinkVault.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Commands;

public record CreateLinkCommand(Guid UserId, string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds) : IRequest<LinkDto>;

public class CreateLinkCommandHandler : IRequestHandler<CreateLinkCommand, LinkDto>
{
    private readonly IAppDbContext _context;
    private readonly IDateTimeProvider _clock;

    public CreateLinkCommandHandler(IAppDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<LinkDto> Handle(CreateLinkCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        // Ownership checks: collection and tags must belong to user
        if (request.CollectionId.HasValue)
        {
            var ownsCollection = await _context.Collections.AnyAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);
            if (!ownsCollection)
            {
                throw new KeyNotFoundException("Collection not found");
            }
        }

        var tagIds = request.TagIds?.Distinct().ToList() ?? new List<Guid>();
        if (tagIds.Count > 0)
        {
            var ownedTags = await _context.Tags.Where(t => t.UserId == request.UserId && tagIds.Contains(t.Id)).Select(t => t.Id).ToListAsync(cancellationToken);
            if (ownedTags.Count != tagIds.Count)
            {
                throw new KeyNotFoundException("One or more tags not found");
            }
        }

        var link = new Link
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CollectionId = request.CollectionId,
            Url = request.Url.Trim(),
            Title = request.Title.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            IsFavorite = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var tagId in tagIds)
        {
            link.LinkTags.Add(new LinkTag { LinkId = link.Id, TagId = tagId });
        }

        _context.Links.Add(link);
        await _context.SaveChangesAsync(cancellationToken);

        // hydrate navigation for dto
        await _context.Entry(link).Collection(l => l.LinkTags).LoadAsync(cancellationToken);

        return link.ToDto();
    }
}
