using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Commands;

public record UpdateLinkCommand(Guid UserId, Guid LinkId, string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds, bool IsFavorite) : IRequest<LinkDto>;

public class UpdateLinkCommandHandler : IRequestHandler<UpdateLinkCommand, LinkDto>
{
    private readonly IAppDbContext _context;
    private readonly IDateTimeProvider _clock;

    public UpdateLinkCommandHandler(IAppDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<LinkDto> Handle(UpdateLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _context.Links
            .Include(l => l.LinkTags)
            .FirstOrDefaultAsync(l => l.Id == request.LinkId && l.UserId == request.UserId, cancellationToken);

        if (link == null)
            throw new KeyNotFoundException("Link not found");

        if (request.CollectionId.HasValue)
        {
            var ownsCollection = await _context.Collections.AnyAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);
            if (!ownsCollection)
                throw new KeyNotFoundException("Collection not found");
        }

        var tagIds = request.TagIds?.Distinct().ToList() ?? new List<Guid>();
        if (tagIds.Count > 0)
        {
            var ownedTags = await _context.Tags.Where(t => t.UserId == request.UserId && tagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);
            if (ownedTags.Count != tagIds.Count)
                throw new KeyNotFoundException("One or more tags not found");
        }

        link.Url = request.Url.Trim();
        link.Title = request.Title.Trim();
        link.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        link.CollectionId = request.CollectionId;
        link.IsFavorite = request.IsFavorite;
        link.UpdatedAt = _clock.UtcNow;

        // update link tags
        link.LinkTags.Clear();
        foreach (var tagId in tagIds)
        {
            link.LinkTags.Add(new Domain.Entities.LinkTag { LinkId = link.Id, TagId = tagId });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return link.ToDto();
    }
}
