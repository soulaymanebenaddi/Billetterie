using Billetterie.Infrastructure;
using Billetterie.Infrastructure.Persistence;
using Billetterie.Infrastructure.Persistence.Seeding;
using Billetterie.Application.Events.GetPublishedEvents;
using Billetterie.Application.Events.GetEventDetails;
using Billetterie.Application.Events.GetEventSeats;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // création de l'objet builder qui permet de configurer l'application

builder.Services.AddOpenApi();

builder.Services.AddControllers(); // notifier .net core qu'on veut utiliser des controllers

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<GetPublishedEvents>();
builder.Services.AddScoped<GetEventDetails>();
builder.Services.AddScoped<GetEventSeats>();

var app = builder.Build();

app.MapControllers(); // routing : Prends les routes définies dans les controllers et les associe aux endpoints HTTP

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BilletterieDbContext>();

    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection(); // redirige les requêtes HTTP vers HTTPS

app.Run();

// Rend le point d'entrée accessible à WebApplicationFactory dans les tests.
public partial class Program { }
