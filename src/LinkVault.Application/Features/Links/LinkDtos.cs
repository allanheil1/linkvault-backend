using LinkVault.Domain.Entities;

namespace LinkVault.Application.Features.Links;

public record LinkDto(Guid Id, string Url, string Title, string? Note, Guid? CollectionId, bool IsFavorite, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, IEnumerable<Guid> TagIds);

public static class LinkMappings
{
    public static LinkDto ToDto(this Link link) =>
        new(link.Id, link.Url, link.Title, link.Note, link.CollectionId, link.IsFavorite, link.CreatedAt, link.UpdatedAt,
            link.LinkTags.Select(lt => lt.TagId));
}
