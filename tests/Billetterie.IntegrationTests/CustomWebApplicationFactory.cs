using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Billetterie.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public DateTimeOffset CurrentTime { get; } =
        new(2030, 6, 15, 12, 0, 0, TimeSpan.Zero);

    public CustomWebApplicationFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Le démarrage en Testing évite les migrations et les données de démonstration de Development.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", _connectionString);

        builder.ConfigureTestServices(services =>
        {
            // Remplace l'horloge système uniquement dans l'application démarrée par les tests.
            services.Replace(ServiceDescriptor.Singleton<TimeProvider>(
                new FixedTimeProvider(CurrentTime)));
        });
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _currentTime;

        public FixedTimeProvider(DateTimeOffset currentTime)
        {
            _currentTime = currentTime;
        }

        public override DateTimeOffset GetUtcNow() => _currentTime;
    }
}
