using LinkVault.Infrastructure.Persistence;
using LinkVault.Infrastructure.Persistence.Seed;
using LinkVault.Infrastructure.Services;
using LinkVault.Infrastructure.Auth;
using LinkVault.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkVault.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<LinkVaultDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                                   ?? "Host=localhost;Port=5432;Database=linkvault;Username=linkvault;Password=linkvault";

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<LinkVaultDbContext>());
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
