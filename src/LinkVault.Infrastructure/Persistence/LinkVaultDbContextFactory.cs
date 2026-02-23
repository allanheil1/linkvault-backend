using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace LinkVault.Infrastructure.Persistence;

public class LinkVaultDbContextFactory : IDesignTimeDbContextFactory<LinkVaultDbContext>
{
    public LinkVaultDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LinkVaultDbContext>();
        var connectionString = "Host=localhost;Port=5432;Database=linkvault;Username=linkvault;Password=linkvault";

        optionsBuilder
            .UseNpgsql(connectionString);

        return new LinkVaultDbContext(optionsBuilder.Options);
    }
}
