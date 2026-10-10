using Billetterie.Application.Events.GetEventSeats;
using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Billetterie.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Billetterie.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class GetEventSeatsTests : IAsyncLifetime
{
    private readonly TestDatabaseFixture _database;

    public GetEventSeatsTests(TestDatabaseFixture database)
    {
        _database = database;
    }

    public Task InitializeAsync() => _database.ResetAsync();

    [Fact]
    public async Task Execute_ReturnsOnlyEventSpaceSeats_WithEventSpecificPrices()
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
        Assert.NotNull(seats);
        Assert.Equal(new[]
        {
            ExpectedSeat(freeSeat, freeRow, freeSection, 0m, "USD"),
            ExpectedSeat(unpricedSeat, unpricedRow, unpricedSection, null, null),
            ExpectedSeat(paidSeat, paidRow, paidSection, 59.99m, "CAD"),
            ExpectedSeat(secondPaidSeat, paidRow, paidSection, 59.99m, "CAD")
        }, seats);

        var otherEventSeats = await GetSeatsAsync(otherEvent.Id);
        Assert.NotNull(otherEventSeats);
        Assert.Equal(new[]
        {
            ExpectedSeat(freeSeat, freeRow, freeSection, 20m, "CAD"),
            ExpectedSeat(unpricedSeat, unpricedRow, unpricedSection, 30m, "USD"),
            ExpectedSeat(paidSeat, paidRow, paidSection, 10m, "EUR"),
            ExpectedSeat(secondPaidSeat, paidRow, paidSection, 10m, "EUR")
        }, otherEventSeats);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_WhenIdDoesNotExist_ReturnsNull(bool useEmptyGuid)
    {
        var venue = CreateVenue();
        var space = new VenueSpace(Guid.NewGuid(), "Salle principale", venue.Id);
        var @event = CreateEvent(space.Id);
        @event.Publish();
        var section = new Section(Guid.NewGuid(), "Parterre", space.Id);
        var row = new Row(Guid.NewGuid(), "A", section.Id);
        var seat = new Seat(Guid.NewGuid(), "01", row.Id);
        await SaveEntitiesAsync(venue, space, @event, section, row, seat);

        var seats = await GetSeatsAsync(useEmptyGuid ? Guid.Empty : Guid.NewGuid());

        Assert.Null(seats);
    }

    [Theory]
    [InlineData(EventStatus.Draft)]
    [InlineData(EventStatus.Cancelled)]
    public async Task Execute_WhenEventIsNotPublished_ReturnsNull(EventStatus status)
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

        Assert.Null(await GetSeatsAsync(@event.Id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Execute_WhenPublishedEventHasNoSeats_ReturnsEmptyList(bool hasRow)
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

        Assert.NotNull(seats);
        Assert.Empty(seats);
    }

    [Theory]
    [InlineData(24)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-24)]
    public async Task Execute_WhenPublishedEventIsFutureStartingNowStartedOrEnded_ReturnsSeats(
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

        Assert.NotNull(seats);
        Assert.Equal(ExpectedSeat(seat, row, section, null, null), Assert.Single(seats));
    }

    [Fact]
    public async Task Execute_ReturnsSeatsOrderedBySectionRowAndLabel_WithoutDuplicates()
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

        Assert.NotNull(seats);
        Assert.NotNull(repeatedSeats);
        Assert.Equal(new[] { firstSeat.Id, secondSeat.Id, thirdSeat.Id, fourthSeat.Id },
            seats.Select(seat => seat.Id));
        Assert.Equal(seats, repeatedSeats);
        Assert.Equal(seats.Count, seats.Select(seat => seat.Id).Distinct().Count());
        Assert.All(seats, seat => Assert.True(seat.IsAvailable));
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

    private async Task<IReadOnlyList<EventSeatDto>?> GetSeatsAsync(Guid eventId)
    {
        await using var scope = _database.Factory.Services.CreateAsyncScope();
        // Passe par le cas d'usage et la vraie requête enregistrée dans l'injection de dépendances.
        var useCase = scope.ServiceProvider.GetRequiredService<GetEventSeats>();
        return await useCase.ExecuteAsync(eventId);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
