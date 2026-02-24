using LinkVault.Application.Common.Interfaces;
using LinkVault.Application.Common.Results;
using LinkVault.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Links.Commands;

public record CreateLinkCommand(Guid UserId, string Url, string Title, string? Note, Guid? CollectionId, IEnumerable<Guid> TagIds) : IRequest<Result<LinkDto>>;

public class CreateLinkCommandHandler : IRequestHandler<CreateLinkCommand, Result<LinkDto>>
{
    private readonly IAppDbContext _context;
    private readonly IDateTimeProvider _clock;

    public CreateLinkCommandHandler(IAppDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<Result<LinkDto>> Handle(CreateLinkCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        // Ownership checks: collection and tags must belong to user
        if (request.CollectionId.HasValue)
        {
            var ownsCollection = await _context.Collections.AnyAsync(c => c.Id == request.CollectionId && c.UserId == request.UserId, cancellationToken);
            if (!ownsCollection)
            {
                return Result<LinkDto>.Failure(new ResultError(ResultErrorType.NotFound, "collection_not_found", "Collection not found."));
            }
        }

        var tagIds = request.TagIds?.Distinct().ToList() ?? new List<Guid>();
        if (tagIds.Count > 0)
        {
            var ownedTags = await _context.Tags.Where(t => t.UserId == request.UserId && tagIds.Contains(t.Id)).Select(t => t.Id).ToListAsync(cancellationToken);
            if (ownedTags.Count != tagIds.Count)
            {
                return Result<LinkDto>.Failure(new ResultError(ResultErrorType.NotFound, "tags_not_found", "One or more tags were not found."));
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

        return Result<LinkDto>.Success(link.ToDto());
    }
}
