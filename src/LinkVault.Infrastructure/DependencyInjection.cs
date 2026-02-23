using LinkVault.Infrastructure.Persistence;
using LinkVault.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace LinkVault.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LinkVaultDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                                   ?? "Host=localhost;Port=5432;Database=linkvault;Username=linkvault;Password=linkvault";

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        return services;
    }
}
