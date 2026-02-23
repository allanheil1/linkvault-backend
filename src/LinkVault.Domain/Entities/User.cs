namespace LinkVault.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
    public ICollection<Link> Links { get; set; } = new List<Link>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
