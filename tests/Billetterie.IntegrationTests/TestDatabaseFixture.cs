using Billetterie.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Billetterie.IntegrationTests;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    public CustomWebApplicationFactory Factory { get; }

    public TestDatabaseFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "BILLETTERIE_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Configure BILLETTERIE_TEST_CONNECTION_STRING before running integration tests.");

        var connectionSettings = new NpgsqlConnectionStringBuilder(connectionString);

        // Le nettoyage ne doit jamais cibler la base de développement.
        if (connectionSettings.Database != "billetterie_tests")
            throw new InvalidOperationException(
                "Integration tests must use the dedicated billetterie_tests database.");

        Factory = new CustomWebApplicationFactory(connectionSettings.ConnectionString);
    }

    public async Task InitializeAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BilletterieDbContext>();

        // Crée la base si nécessaire et applique les migrations existantes, sans données de démonstration.
        await dbContext.Database.MigrateAsync();
    }

    public async Task ResetAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BilletterieDbContext>();

        // Toutes les tables liées sont vidées ensemble ; l'historique des migrations est conservé.
        await dbContext.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE "EventSectionPrices", "Events", "Seats", "Rows",
                "Sections", "VenueSpaces", "Venues";
            """);
    }

    public Task DisposeAsync() => Factory.DisposeAsync().AsTask();
}

// Les classes de cette collection partagent la base et s'exécutent sans se chevaucher.
[CollectionDefinition(PostgreSqlCollection.Name, DisableParallelization = true)]
public sealed class PostgreSqlCollection : ICollectionFixture<TestDatabaseFixture>
{
    public const string Name = "PostgreSQL";
}
