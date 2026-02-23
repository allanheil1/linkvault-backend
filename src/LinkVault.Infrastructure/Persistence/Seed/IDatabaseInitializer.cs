namespace LinkVault.Infrastructure.Persistence.Seed;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
