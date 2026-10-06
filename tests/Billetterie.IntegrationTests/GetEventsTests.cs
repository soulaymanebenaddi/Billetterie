using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Billetterie.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class GetEventsTests : IAsyncLifetime
{
    private readonly TestDatabaseFixture _database;
    private readonly HttpClient _client;

    public GetEventsTests(TestDatabaseFixture database)
    {
        _database = database;
        _client = database.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Utilise HTTPS dans le serveur de test pour éviter la redirection de l'API.
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    public Task InitializeAsync() => _database.ResetAsync();

    [Fact]
    public async Task Get_WhenCatalogIsEmpty_ReturnsOkWithEmptyJsonArray()
    {
        using var response = await _client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Equal(0, json.RootElement.GetArrayLength());
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }
}
