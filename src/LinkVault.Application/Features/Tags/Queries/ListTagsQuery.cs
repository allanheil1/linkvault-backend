using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Tags.Queries;

public record ListTagsQuery(Guid UserId) : IRequest<IEnumerable<TagDto>>;

public class ListTagsQueryHandler : IRequestHandler<ListTagsQuery, IEnumerable<TagDto>>
{
    private readonly IAppDbContext _context;

    public ListTagsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TagDto>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _context.Tags.AsNoTracking()
            .Where(t => t.UserId == request.UserId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return tags.Select(t => new TagDto(t.Id, t.Name));
    }
}
