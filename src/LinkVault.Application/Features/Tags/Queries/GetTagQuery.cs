using LinkVault.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Tags.Queries;

public record GetTagQuery(Guid UserId, Guid TagId) : IRequest<TagDto>;

public class GetTagQueryHandler : IRequestHandler<GetTagQuery, TagDto?>
{
    private readonly IAppDbContext _context;

    public GetTagQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TagDto?> Handle(GetTagQuery request, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TagId && t.UserId == request.UserId, cancellationToken);
        return tag is null ? null : new TagDto(tag.Id, tag.Name);
    }
}
