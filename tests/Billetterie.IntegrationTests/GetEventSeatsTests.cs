using System.Data.Common;
using System.Net;
using System.Text.Json;
using Billetterie.Application.Events.GetEventSeats;
using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Billetterie.Infrastructure.Persistence;
using Billetterie.Infrastructure.Persistence.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Billetterie.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class GetEventSeatsTests : IAsyncLifetime
{
    private readonly TestDatabaseFixture _database;
    private readonly HttpClient _client;

    public GetEventSeatsTests(TestDatabaseFixture database)
    {
        _database = database;
        _client = database.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    public Task InitializeAsync() => _database.ResetAsync();

    [Fact]
    public async Task Get_ReturnsOnlyEventSpaceSeats_WithEventSpecificPrices()
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var otherSpace = new VenueSpace(Guid.NewGuid(), "Autre salle", venue.Id);
        var otherVenue = new Venue(Guid.NewGuid(), "Autre lieu", "200 rue du Test", "Québec");
        var otherVenueSpace = new VenueSpace(Guid.NewGuid(), "Auditorium", otherVenue.Id);
        var @event = CreateEvent(space.Id);
        var otherEvent = CreateEvent(space.Id);
        @event.Publish();
        otherEvent.Publish();

        var paidSection = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var freeSection = new Section(Guid.NewGuid(), "Balcon", space.Id);
        var unpricedSection = new Section(Guid.NewGuid(), "Loges", space.Id);
        var paidRow = new Row(Guid.NewGuid(), "A", paidSection.Id);
        var freeRow = new Row(Guid.NewGuid(), "A", freeSection.Id);
        var unpricedRow = new Row(Guid.NewGuid(), "A", unpricedSection.Id);
        var paidSeat = new Seat(Guid.NewGuid(), "01", paidRow.Id);
        var secondPaidSeat = new Seat(Guid.NewGuid(), "02", paidRow.Id);
        var freeSeat = new Seat(Guid.NewGuid(), "01", freeRow.Id);
        var unpricedSeat = new Seat(Guid.NewGuid(), "01", unpricedRow.Id);

        var otherSection = new Section(Guid.NewGuid(), "Parterre", otherSpace.Id);
        var otherRow = new Row(Guid.NewGuid(), "A", otherSection.Id);
        var otherSeat = new Seat(Guid.NewGuid(), "01", otherRow.Id);
        var otherVenueSection = new Section(Guid.NewGuid(), "Parterre", otherVenueSpace.Id);
        var otherVenueRow = new Row(Guid.NewGuid(), "A", otherVenueSection.Id);
        var otherVenueSeat = new Seat(Guid.NewGuid(), "01", otherVenueRow.Id);

        await SaveEntitiesAsync(venue, space, otherSpace, otherVenue, otherVenueSpace,
            @event, otherEvent, paidSection, freeSection, unpricedSection,
            paidRow, freeRow, unpricedRow, paidSeat, secondPaidSeat, freeSeat, unpricedSeat,
            otherSection, otherRow, otherSeat, otherVenueSection, otherVenueRow, otherVenueSeat,
            EventSectionPrice.Create(Guid.NewGuid(), @event, paidSection, 59.99m, "CAD"),
            EventSectionPrice.Create(Guid.NewGuid(), @event, freeSection, 0m, "USD"),
            EventSectionPrice.Create(Guid.NewGuid(), otherEvent, paidSection, 10m, "EUR"),
            EventSectionPrice.Create(Guid.NewGuid(), otherEvent, freeSection, 20m, "CAD"),
            // Cette section est tarifée uniquement pour l'autre événement.
            EventSectionPrice.Create(Guid.NewGuid(), otherEvent, unpricedSection, 30m, "USD"));

        var seats = await GetSeatsAsync(@event.Id);
        AssertSeats(seats,
            ExpectedSeat(freeSeat, freeRow, freeSection, 0m, "USD"),
            ExpectedSeat(unpricedSeat, unpricedRow, unpricedSection, null, null),
            ExpectedSeat(paidSeat, paidRow, paidSection, 59.99m, "CAD"),
            ExpectedSeat(secondPaidSeat, paidRow, paidSection, 59.99m, "CAD"));

        var otherEventSeats = await GetSeatsAsync(otherEvent.Id);
        AssertSeats(otherEventSeats,
            ExpectedSeat(freeSeat, freeRow, freeSection, 20m, "CAD"),
            ExpectedSeat(unpricedSeat, unpricedRow, unpricedSection, 30m, "USD"),
            ExpectedSeat(paidSeat, paidRow, paidSection, 10m, "EUR"),
            ExpectedSeat(secondPaidSeat, paidRow, paidSection, 10m, "EUR"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_WhenIdDoesNotExist_ReturnsNotFound(bool useEmptyGuid)
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent(space.Id);
        @event.Publish();
        var section = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var row = new Row(Guid.NewGuid(), "A", section.Id);
        var seat = new Seat(Guid.NewGuid(), "01", row.Id);
        await SaveEntitiesAsync(venue, space, @event, section, row, seat);

        var unknownId = useEmptyGuid ? Guid.Empty : Guid.NewGuid();
        using var response = await _client.GetAsync($"/api/events/{unknownId}/seats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(EventStatus.Draft)]
    [InlineData(EventStatus.Cancelled)]
    public async Task Get_WhenEventIsNotPublished_ReturnsNotFound(EventStatus status)
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent(space.Id);
        if (status == EventStatus.Cancelled)
        {
            @event.Publish();
            @event.Cancel();
        }
        var section = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var row = new Row(Guid.NewGuid(), "A", section.Id);
        var seat = new Seat(Guid.NewGuid(), "01", row.Id);
        await SaveEntitiesAsync(venue, space, @event, section, row, seat);

        using var response = await _client.GetAsync($"/api/events/{@event.Id}/seats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("123")]
    public async Task Get_WhenIdIsMalformed_ReturnsBadRequest(string id)
    {
        using var response = await _client.GetAsync($"/api/events/{id}/seats");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_WhenPublishedEventHasNoSeats_ReturnsEmptyArray(bool hasRow)
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent(space.Id);
        @event.Publish();
        await SaveEntitiesAsync(venue, space, @event);
        if (hasRow)
        {
            var section = new Section(Guid.NewGuid(), "Parterre", space.Id);
            var row = new Row(Guid.NewGuid(), "A", section.Id);
            await SaveEntitiesAsync(section, row);
        }

        var seats = await GetSeatsAsync(@event.Id);

        AssertSeats(seats);
    }

    [Theory]
    [InlineData(24)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-24)]
    public async Task Get_WhenPublishedEventIsFutureStartingNowStartedOrEnded_ReturnsSeats(
        int startOffsetHours)
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent(space.Id, startOffsetHours);
        @event.Publish();
        var section = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var row = new Row(Guid.NewGuid(), "A", section.Id);
        var seat = new Seat(Guid.NewGuid(), "01", row.Id);
        await SaveEntitiesAsync(venue, space, @event, section, row, seat);

        var seats = await GetSeatsAsync(@event.Id);

        AssertSeats(seats, ExpectedSeat(seat, row, section, null, null));
    }

    [Fact]
    public async Task Get_ReturnsSeatsOrderedBySectionRowAndLabel_WithoutDuplicates()
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent(space.Id);
        @event.Publish();
        var firstSection = new Section(Guid.NewGuid(), "Balcon", space.Id);
        var lastSection = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var firstRow = new Row(Guid.NewGuid(), "A", firstSection.Id);
        var lastRow = new Row(Guid.NewGuid(), "B", firstSection.Id);
        var otherSectionRow = new Row(Guid.NewGuid(), "A", lastSection.Id);
        var firstSeat = new Seat(Guid.NewGuid(), "01", firstRow.Id);
        var secondSeat = new Seat(Guid.NewGuid(), "02", firstRow.Id);
        var thirdSeat = new Seat(Guid.NewGuid(), "01", lastRow.Id);
        var fourthSeat = new Seat(Guid.NewGuid(), "01", otherSectionRow.Id);
        // Insertion inversée pour éviter que l'ordre d'insertion suffise à faire passer le test.
        await SaveEntitiesAsync(venue, space, @event, lastSection, firstSection,
            otherSectionRow, lastRow, firstRow, fourthSeat, thirdSeat, secondSeat, firstSeat);

        var seats = await GetSeatsAsync(@event.Id);
        var repeatedSeats = await GetSeatsAsync(@event.Id);

        var expectedSeats = new[]
        {
            ExpectedSeat(firstSeat, firstRow, firstSection, null, null),
            ExpectedSeat(secondSeat, firstRow, firstSection, null, null),
            ExpectedSeat(thirdSeat, lastRow, firstSection, null, null),
            ExpectedSeat(fourthSeat, otherSectionRow, lastSection, null, null)
        };
        AssertSeats(seats, expectedSeats);
        AssertSeats(repeatedSeats, expectedSeats);
        Assert.Equal(seats.GetArrayLength(),
            seats.EnumerateArray().Select(seat => seat.GetProperty("id").GetGuid()).Distinct().Count());
    }

    [Fact]
    public async Task Execute_WhenEventIsCancelledAfterReadStarts_ReturnsConsistentSnapshot()
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent(space.Id);
        @event.Publish();
        var section = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var row = new Row(Guid.NewGuid(), "A", section.Id);
        var seat = new Seat(Guid.NewGuid(), "01", row.Id);
        await SaveEntitiesAsync(venue, space, @event, section, row, seat,
            EventSectionPrice.Create(Guid.NewGuid(), @event, section, 59.99m, "CAD"));

        await using var writeScope = _database.Factory.Services.CreateAsyncScope();
        var writeContext = writeScope.ServiceProvider.GetRequiredService<BilletterieDbContext>();
        var interceptor = new CancelEventAfterReadInterceptor(writeContext, @event.Id);
        var options = new DbContextOptionsBuilder<BilletterieDbContext>()
            .UseNpgsql(writeContext.Database.GetConnectionString())
            .AddInterceptors(interceptor)
            .Options;
        await using var readContext = new BilletterieDbContext(options);
        var useCase = new GetEventSeats(new EventSeatQuery(readContext));

        var seats = await useCase.ExecuteAsync(@event.Id);

        // La modification est validée après le début du SELECT, avant sa matérialisation.
        // Avec AnyAsync suivi de ToListAsync, la seconde lecture renverrait une liste vide.
        Assert.NotNull(seats);
        Assert.Equal(ExpectedSeat(seat, row, section, 59.99m, "CAD"), Assert.Single(seats));
        Assert.Equal(1, interceptor.ReadCount);
        // Une nouvelle lecture observe bien l'annulation déjà validée.
        Assert.Null(await ExecuteGetSeatsAsync(@event.Id));
    }

    private sealed class CancelEventAfterReadInterceptor(
        BilletterieDbContext writeContext, Guid eventId) : DbCommandInterceptor
    {
        public int ReadCount { get; private set; }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            ReadCount++;
            if (ReadCount == 1)
            {
                var @event = await writeContext.Events.SingleAsync(
                    @event => @event.Id == eventId, cancellationToken);
                @event.Cancel();
                await writeContext.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }

    private static Venue CreateVenue() =>
        new(Guid.NewGuid(), "Théâtre Montréal", "100 rue du Test", "Montréal");

    private Event CreateEvent(Guid venueSpaceId, int startOffsetHours = 24)
    {
        var startsAt = _database.Factory.CurrentTime.AddHours(startOffsetHours);
        return new Event(Guid.NewGuid(), "Événement de test", null, startsAt,
            startsAt.AddHours(2), null, EventCategory.Theatre, venueSpaceId);
    }

    private static EventSeatDto ExpectedSeat(
        Seat seat, Row row, Section section, decimal? price, string? currency) =>
        new(seat.Id, seat.Label, row.Id, row.Name, section.Id, section.Name, price, currency, true);

    private async Task SaveEntitiesAsync(params object[] entities)
    {
        await using var scope = _database.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BilletterieDbContext>();
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    private async Task<JsonElement> GetSeatsAsync(Guid eventId)
    {
        using var response = await _client.GetAsync($"/api/events/{eventId}/seats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);

        // Le document est libéré ici ; le clone reste utilisable par les assertions du scénario.
        return json.RootElement.Clone();
    }

    private static void AssertSeats(JsonElement seats, params EventSeatDto[] expectedSeats)
    {
        Assert.Equal(expectedSeats.Length, seats.GetArrayLength());
        for (var index = 0; index < expectedSeats.Length; index++)
        {
            var item = seats[index];
            var expected = expectedSeats[index];
            Assert.Equal(JsonValueKind.Object, item.ValueKind);
            Assert.Equal(expected.Id, item.GetProperty("id").GetGuid());
            Assert.Equal(expected.Label, item.GetProperty("label").GetString());
            Assert.Equal(expected.RowId, item.GetProperty("rowId").GetGuid());
            Assert.Equal(expected.RowName, item.GetProperty("rowName").GetString());
            Assert.Equal(expected.SectionId, item.GetProperty("sectionId").GetGuid());
            Assert.Equal(expected.SectionName, item.GetProperty("sectionName").GetString());
            if (expected.Price is { } price)
            {
                Assert.Equal(price, item.GetProperty("price").GetDecimal());
            }
            else
            {
                Assert.Equal(JsonValueKind.Null, item.GetProperty("price").ValueKind);
            }

            Assert.Equal(expected.Currency, item.GetProperty("currency").GetString());
            if (expected.Currency is null)
            {
                Assert.Equal(JsonValueKind.Null, item.GetProperty("currency").ValueKind);
            }

            Assert.Equal(expected.IsAvailable, item.GetProperty("isAvailable").GetBoolean());
        }
    }

    private async Task<IReadOnlyList<EventSeatDto>?> ExecuteGetSeatsAsync(Guid eventId)
    {
        await using var scope = _database.Factory.Services.CreateAsyncScope();
        // Passe par le cas d'usage et la vraie requête enregistrée dans l'injection de dépendances.
        var useCase = scope.ServiceProvider.GetRequiredService<GetEventSeats>();
        return await useCase.ExecuteAsync(eventId);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }
}
