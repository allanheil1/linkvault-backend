using LinkVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Infrastructure.Persistence;

public class LinkVaultDbContext : DbContext, LinkVault.Application.Common.Interfaces.IAppDbContext
{
    public LinkVaultDbContext(DbContextOptions<LinkVaultDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Link> Links => Set<Link>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<LinkTag> LinkTags => Set<LinkTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LinkVaultDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
