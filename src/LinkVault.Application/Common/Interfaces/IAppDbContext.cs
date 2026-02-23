using LinkVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Collection> Collections { get; }
    DbSet<Link> Links { get; }
    DbSet<Tag> Tags { get; }
    DbSet<LinkTag> LinkTags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
