using Billetterie.Infrastructure;

var builder = WebApplication.CreateBuilder(args); // création de l'objet builder qui permet de configurer l'application

builder.Services.AddOpenApi();

builder.Services.AddControllers(); // notifier .net core qu'on veut utiliser des controllers

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapControllers(); // routing : Prends les routes définies dans les controllers et les associe aux endpoints HTTP

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection(); // redirige les requêtes HTTP vers HTTPS

app.Run();
