namespace LinkVault.Domain.Entities;

public class Collection
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Link> Links { get; set; } = new List<Link>();
}
