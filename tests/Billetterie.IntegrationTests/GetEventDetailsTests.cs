using System.Net;
using System.Text.Json;
using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Billetterie.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Billetterie.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class GetEventDetailsTests : IAsyncLifetime
{
    private readonly TestDatabaseFixture _database;
    private readonly HttpClient _client;

    public GetEventDetailsTests(TestDatabaseFixture database)
    {
        _database = database;
        _client = database.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    public Task InitializeAsync() => _database.ResetAsync();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ReturnsRequestedEventDetailsAndLowestSectionPrice_IncludingZero(
        bool hasFreeSection)
    {
        var venue = new Venue(Guid.NewGuid(), "Théâtre Montréal", "100 rue du Test", "Montréal");
        var otherSpace = new VenueSpace(Guid.NewGuid(), "Petite salle", venue.Id);
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent("Pièce de théâtre",
            _database.Factory.CurrentTime.AddDays(1).ToOffset(TimeSpan.FromHours(-4)),
            space.Id, "Une pièce à découvrir.", "/images/events/theatre.webp", EventCategory.Theatre);
        @event.Publish();

        var balcony = new Section(Guid.NewGuid(), "Balcon", space.Id);
        var floor = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var boxes = new Section(Guid.NewGuid(), "Loges", space.Id);
        var lowestAmount = hasFreeSection ? 0m : 29.99m;
        var floorPrice = EventSectionPrice.Create(Guid.NewGuid(), @event, floor, 75m, "CAD");
        // Des devises distinctes vérifient que le montant et la devise viennent du même tarif.
        var balconyPrice = EventSectionPrice.Create(Guid.NewGuid(), @event, balcony, lowestAmount, "USD");
        var boxesPrice = EventSectionPrice.Create(Guid.NewGuid(), @event, boxes, 49.99m, "EUR");

        var otherVenue = new Venue(Guid.NewGuid(), "Centre Québec", "200 rue du Test", "Québec");
        var otherVenueSpace = new VenueSpace(Guid.NewGuid(), "Auditorium", otherVenue.Id);
        var otherEvent = CreateEvent("Autre événement", _database.Factory.CurrentTime.AddDays(2),
            otherVenueSpace.Id);
        otherEvent.Publish();
        var otherSection = new Section(Guid.NewGuid(), "Section", otherVenueSpace.Id);
        var otherPrice = EventSectionPrice.Create(Guid.NewGuid(), otherEvent, otherSection, 1m, "CAD");

        await SaveEntitiesAsync(venue, otherSpace, space, @event, balcony, floor, boxes,
            floorPrice, balconyPrice, boxesPrice, otherVenue, otherVenueSpace,
            otherEvent, otherSection, otherPrice);

        using var response = await _client.GetAsync($"/api/events/{@event.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var item = json.RootElement;
        AssertEventDetails(item, @event, venue, space);
        Assert.Equal(lowestAmount, item.GetProperty("startingPrice").GetDecimal());
        Assert.Equal("USD", item.GetProperty("currency").GetString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_WhenIdDoesNotExist_ReturnsNotFound(bool useEmptyGuid)
    {
        // Un événement visible existe pour vérifier que l'API ne le retourne pas à la place.
        var venue = new Venue(Guid.NewGuid(), "Théâtre Montréal", "100 rue du Test", "Montréal");
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent("Événement publié", _database.Factory.CurrentTime.AddDays(1), space.Id);
        @event.Publish();
        await SaveEntitiesAsync(venue, space, @event);

        var unknownId = useEmptyGuid ? Guid.Empty : Guid.NewGuid();
        using var response = await _client.GetAsync($"/api/events/{unknownId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(EventStatus.Draft)]
    [InlineData(EventStatus.Cancelled)]
    public async Task Get_WhenEventIsNotPublished_ReturnsNotFound(EventStatus status)
    {
        var venue = new Venue(Guid.NewGuid(), "Théâtre Montréal", "100 rue du Test", "Montréal");
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent("Événement non visible", _database.Factory.CurrentTime.AddDays(1), space.Id);
        if (status == EventStatus.Cancelled)
        {
            @event.Publish();
            @event.Cancel();
        }
        await SaveEntitiesAsync(venue, space, @event);

        using var response = await _client.GetAsync($"/api/events/{@event.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-24)]
    public async Task Get_WhenPublishedEventHasStartedOrEnded_ReturnsOk(int startOffsetHours)
    {
        var venue = new Venue(Guid.NewGuid(), "Théâtre Montréal", "100 rue du Test", "Montréal");
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent("Événement publié",
            _database.Factory.CurrentTime.AddHours(startOffsetHours), space.Id);
        @event.Publish();
        await SaveEntitiesAsync(venue, space, @event);

        using var response = await _client.GetAsync($"/api/events/{@event.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        AssertEventDetails(json.RootElement, @event, venue, space);
    }

    [Fact]
    public async Task Get_WhenEventHasNoPricesOrOptionalContent_ReturnsDetailsWithNullFields()
    {
        var venue = new Venue(Guid.NewGuid(), "Centre Québec", "200 rue du Test", "Québec");
        var space = new VenueSpace(Guid.NewGuid(), "Auditorium", venue.Id);
        var section = new Section(Guid.NewGuid(), "Section sans tarif", space.Id);
        var @event = CreateEvent("Conférence sans tarif", _database.Factory.CurrentTime.AddDays(1),
            space.Id, category: EventCategory.Conference);
        @event.Publish();
        await SaveEntitiesAsync(venue, space, section, @event);

        using var response = await _client.GetAsync($"/api/events/{@event.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var item = json.RootElement;
        AssertEventDetails(item, @event, venue, space);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("description").ValueKind);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("imageUrl").ValueKind);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("startingPrice").ValueKind);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("currency").ValueKind);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Get_WhenIdIsMalformed_ReturnsBadRequest(string id)
    {
        using var response = await _client.GetAsync($"/api/events/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static Event CreateEvent(
        string name,
        DateTimeOffset startsAt,
        Guid venueSpaceId,
        string? description = null,
        string? imageUrl = null,
        EventCategory category = EventCategory.Concert)
    {
        return new Event(Guid.NewGuid(), name, description, startsAt, startsAt.AddHours(2),
            imageUrl, category, venueSpaceId);
    }

    private async Task SaveEntitiesAsync(params object[] entities)
    {
        await using var scope = _database.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BilletterieDbContext>();

        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    private static void AssertEventDetails(JsonElement item, Event @event, Venue venue, VenueSpace space)
    {
        Assert.Equal(JsonValueKind.Object, item.ValueKind);
        Assert.Equal(@event.Id, item.GetProperty("id").GetGuid());
        Assert.Equal(@event.Name, item.GetProperty("name").GetString());
        Assert.Equal(@event.Description, item.GetProperty("description").GetString());
        Assert.Equal(@event.ImageUrl, item.GetProperty("imageUrl").GetString());
        Assert.Equal((int)@event.Category, item.GetProperty("category").GetInt32());
        Assert.Equal(@event.StartsAt, item.GetProperty("startsAt").GetDateTimeOffset());
        Assert.Equal(@event.EndsAt, item.GetProperty("endsAt").GetDateTimeOffset());
        Assert.Equal(venue.Name, item.GetProperty("venueName").GetString());
        Assert.Equal(venue.Address, item.GetProperty("address").GetString());
        Assert.Equal(venue.City, item.GetProperty("city").GetString());
        Assert.Equal(space.Name, item.GetProperty("venueSpaceName").GetString());
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }
}
