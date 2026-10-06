using System.Net;
using System.Text.Json;
using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Billetterie.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

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

    [Fact]
    public async Task Get_ReturnsOnlyPublishedFutureEvents_InChronologicalOrder()
    {
        var currentTime = _database.Factory.CurrentTime;
        var venue = new Venue(Guid.NewGuid(), "Théâtre Montréal", "100 rue du Test", "Montréal");
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);

        var laterEvent = CreateEvent("Événement plus tard", currentTime.AddDays(3), space.Id);
        laterEvent.Publish();
        var earlierEvent = CreateEvent("Événement plus proche", currentTime.AddDays(1), space.Id);
        earlierEvent.Publish();
        var draftEvent = CreateEvent("Brouillon futur", currentTime.AddDays(1), space.Id);
        var cancelledEvent = CreateEvent("Événement annulé", currentTime.AddDays(2), space.Id);
        cancelledEvent.Publish();
        cancelledEvent.Cancel();
        var pastEvent = CreateEvent("Événement passé", currentTime.AddDays(-1), space.Id);
        pastEvent.Publish();
        var startingNowEvent = CreateEvent("Événement à l'heure courante", currentTime, space.Id);
        startingNowEvent.Publish();

        // Le plus lointain est ajouté avant le plus proche pour vérifier le tri de la requête.
        await SaveEntitiesAsync(venue, space, laterEvent, earlierEvent,
            draftEvent, cancelledEvent, pastEvent, startingNowEvent);

        using var response = await _client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Collection(json.RootElement.EnumerateArray(),
            item => Assert.Equal(earlierEvent.Id, item.GetProperty("id").GetGuid()),
            item => Assert.Equal(laterEvent.Id, item.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task Get_ReturnsEventDetailsAndLowestSectionPrice_IncludingZero()
    {
        var currentTime = _database.Factory.CurrentTime;
        var paidVenue = new Venue(Guid.NewGuid(), "Théâtre Montréal", "100 rue du Test", "Montréal");
        var paidSpace = new VenueSpace(Guid.NewGuid(), "Salle de théâtre", paidVenue.Id);
        var paidEvent = CreateEvent("Pièce de théâtre", currentTime.AddDays(1), paidSpace.Id,
            EventCategory.Theatre, "/images/events/theatre.webp");
        paidEvent.Publish();
        var balcony = new Section(Guid.NewGuid(), "Balcon", paidSpace.Id);
        var floor = new Section(Guid.NewGuid(), "Parterre", paidSpace.Id);
        var boxes = new Section(Guid.NewGuid(), "Loges", paidSpace.Id);
        var floorPrice = EventSectionPrice.Create(Guid.NewGuid(), paidEvent, floor, 75m, "CAD");
        var balconyPrice = EventSectionPrice.Create(Guid.NewGuid(), paidEvent, balcony, 29.99m, "CAD");
        var boxesPrice = EventSectionPrice.Create(Guid.NewGuid(), paidEvent, boxes, 49.99m, "CAD");

        var freeVenue = new Venue(Guid.NewGuid(), "Centre Québec", "200 rue du Test", "Québec");
        var freeSpace = new VenueSpace(Guid.NewGuid(), "Auditorium", freeVenue.Id);
        var freeEvent = CreateEvent("Conférence avec section gratuite", currentTime.AddDays(2),
            freeSpace.Id, EventCategory.Conference);
        freeEvent.Publish();
        var freeSection = new Section(Guid.NewGuid(), "Accès gratuit", freeSpace.Id);
        var premiumSection = new Section(Guid.NewGuid(), "Accès premium", freeSpace.Id);
        var freePrice = EventSectionPrice.Create(Guid.NewGuid(), freeEvent, freeSection, 0m, "CAD");
        var premiumPrice = EventSectionPrice.Create(Guid.NewGuid(), freeEvent, premiumSection, 10m, "CAD");

        await SaveEntitiesAsync(paidVenue, paidSpace, paidEvent, balcony, floor, boxes,
            floorPrice, balconyPrice, boxesPrice, freeVenue, freeSpace, freeEvent,
            freeSection, premiumSection, freePrice, premiumPrice);

        using var response = await _client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Plusieurs sections tarifées doivent produire une seule entrée par événement.
        Assert.Equal(2, json.RootElement.GetArrayLength());
        var paidItem = json.RootElement[0];
        AssertEventDetails(paidItem, paidEvent, paidVenue);
        Assert.Equal(29.99m, paidItem.GetProperty("startingPrice").GetDecimal());
        Assert.Equal("CAD", paidItem.GetProperty("currency").GetString());

        var freeItem = json.RootElement[1];
        AssertEventDetails(freeItem, freeEvent, freeVenue);
        Assert.Equal(0m, freeItem.GetProperty("startingPrice").GetDecimal());
        Assert.Equal("CAD", freeItem.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task Get_WhenEventHasNoPrices_ReturnsEventWithNullPriceAndCurrency()
    {
        var venue = new Venue(Guid.NewGuid(), "Centre Québec", "200 rue du Test", "Québec");
        var space = new VenueSpace(Guid.NewGuid(), "Auditorium", venue.Id);
        var section = new Section(Guid.NewGuid(), "Section sans tarif", space.Id);
        var @event = CreateEvent("Conférence sans tarif", _database.Factory.CurrentTime.AddDays(1),
            space.Id, EventCategory.Conference);
        @event.Publish();

        await SaveEntitiesAsync(venue, space, section, @event);

        using var response = await _client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var item = Assert.Single(json.RootElement.EnumerateArray());
        AssertEventDetails(item, @event, venue);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("startingPrice").ValueKind);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("currency").ValueKind);
    }

    private static Event CreateEvent(
        string name,
        DateTimeOffset startsAt,
        Guid venueSpaceId,
        EventCategory category = EventCategory.Concert,
        string? imageUrl = null)
    {
        return new Event(Guid.NewGuid(), name, null, startsAt, startsAt.AddHours(2),
            imageUrl, category, venueSpaceId);
    }

    private async Task SaveEntitiesAsync(params object[] entities)
    {
        await using var scope = _database.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BilletterieDbContext>();

        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    private static void AssertEventDetails(JsonElement item, Event @event, Venue venue)
    {
        Assert.Equal(@event.Id, item.GetProperty("id").GetGuid());
        Assert.Equal(@event.Name, item.GetProperty("name").GetString());
        Assert.Equal(@event.ImageUrl, item.GetProperty("imageUrl").GetString());
        Assert.Equal((int)@event.Category, item.GetProperty("category").GetInt32());
        Assert.Equal(@event.StartsAt, item.GetProperty("startsAt").GetDateTimeOffset());
        Assert.Equal(venue.Name, item.GetProperty("venueName").GetString());
        Assert.Equal(venue.City, item.GetProperty("city").GetString());
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }
}
