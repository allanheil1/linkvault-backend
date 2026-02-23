namespace LinkVault.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;

    public User User { get; set; } = null!;
    public ICollection<LinkTag> LinkTags { get; set; } = new List<LinkTag>();
}
