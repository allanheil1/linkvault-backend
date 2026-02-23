namespace LinkVault.Domain.Entities;

public class Link
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CollectionId { get; set; }
    public string Url { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Note { get; set; }
    public bool IsFavorite { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Collection? Collection { get; set; }
    public ICollection<LinkTag> LinkTags { get; set; } = new List<LinkTag>();
}
