using Billetterie.Infrastructure.Persistence;
using Billetterie.Infrastructure.Persistence.Seeding;
using Billetterie.Infrastructure.Persistence.Queries;
using Billetterie.Application.Events.GetPublishedEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Billetterie.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Database connection string is not configured.");

        services.AddDbContext<BilletterieDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<DevelopmentDataSeeder>();

        services.AddScoped<IEventCatalogQuery, EventCatalogQuery>();

        return services;
    }
}
