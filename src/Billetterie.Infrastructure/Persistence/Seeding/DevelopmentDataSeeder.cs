using System.Security.Cryptography;
using System.Text;
using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Billetterie.Infrastructure.Persistence.Seeding;

public sealed class DevelopmentDataSeeder
{
    private const string Currency = "CAD";
    private const string IdNamespace = "billetterie-development-seed";
    private const string VenueTimeZoneId = "America/Toronto";

    private readonly BilletterieDbContext _dbContext;

    public DevelopmentDataSeeder(BilletterieDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedData = CreateSeedData(DateTimeOffset.UtcNow);

        ValidatePricingConsistency(seedData);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await AddMissingAsync(
            _dbContext.Venues,
            seedData.Venues,
            venue => venue.Id,
            cancellationToken);
        await AddMissingAsync(
            _dbContext.VenueSpaces,
            seedData.VenueSpaces,
            venueSpace => venueSpace.Id,
            cancellationToken);
        await AddMissingAsync(
            _dbContext.Sections,
            seedData.Sections,
            section => section.Id,
            cancellationToken);
        await AddMissingAsync(
            _dbContext.Rows,
            seedData.Rows,
            row => row.Id,
            cancellationToken);
        await AddMissingAsync(
            _dbContext.Seats,
            seedData.Seats,
            seat => seat.Id,
            cancellationToken);
        await AddMissingAsync(
            _dbContext.Events,
            seedData.Events,
            @event => @event.Id,
            cancellationToken);
        await AddMissingAsync(
            _dbContext.EventSectionPrices,
            seedData.EventSectionPrices,
            price => price.Id,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task AddMissingAsync<TEntity>(
        DbSet<TEntity> dbSet,
        IReadOnlyCollection<TEntity> entities,
        Func<TEntity, Guid> idSelector,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var expectedIds = entities
            .Select(idSelector)
            .ToArray();

        var existingIds = (await dbSet
                .AsNoTracking()
                .Where(entity => expectedIds.Contains(EF.Property<Guid>(entity, "Id")))
                .Select(entity => EF.Property<Guid>(entity, "Id"))
                .ToListAsync(cancellationToken))
            .ToHashSet();

        dbSet.AddRange(
            entities.Where(entity =>
                !existingIds.Contains(idSelector(entity))));
    }

    private static SeedData CreateSeedData(DateTimeOffset currentTime)
    {
        var venues = new List<Venue>();
        var venueSpaces = new List<VenueSpace>();
        var sections = new List<Section>();
        var rows = new List<Row>();
        var seats = new List<Seat>();
        var events = new List<Event>();
        var eventSectionPrices = new List<EventSectionPrice>();

        var theatre = AddVenue(
            "theatre-maisonneuve",
            "Théâtre Maisonneuve",
            "175 rue Sainte-Catherine Ouest",
            "Montréal");
        var theatreMainHall = AddVenueSpace(
            "theatre-maisonneuve-main-hall",
            "Grande salle",
            theatre);
        var orchestra = AddSection(
            "theatre-maisonneuve-main-hall-orchestra",
            "Parterre",
            theatreMainHall,
            rowNames: ["A", "B", "C"],
            seatsPerRow: 8);
        var balcony = AddSection(
            "theatre-maisonneuve-main-hall-balcony",
            "Balcon",
            theatreMainHall,
            rowNames: ["D", "E"],
            seatsPerRow: 8);

        var arena = AddVenue(
            "arena-du-fleuve",
            "Aréna du Fleuve",
            "250 boulevard des Rives",
            "Québec");
        var arenaMainFloor = AddVenueSpace(
            "arena-du-fleuve-main-floor",
            "Aréna principale",
            arena);
        var lowerBowl = AddSection(
            "arena-du-fleuve-main-floor-level-100",
            "Niveau 100",
            arenaMainFloor,
            rowNames: ["A", "B", "C"],
            seatsPerRow: 10);
        var upperBowl = AddSection(
            "arena-du-fleuve-main-floor-level-200",
            "Niveau 200",
            arenaMainFloor,
            rowNames: ["D", "E", "F"],
            seatsPerRow: 10);

        var conventionCentre = AddVenue(
            "centre-horizon",
            "Centre Horizon",
            "800 avenue du Sommet",
            "Laval");
        var auditorium = AddVenueSpace(
            "centre-horizon-auditorium",
            "Auditorium",
            conventionCentre);
        var auditoriumFloor = AddSection(
            "centre-horizon-auditorium-floor",
            "Orchestre",
            auditorium,
            rowNames: ["A", "B", "C"],
            seatsPerRow: 8);
        var mezzanine = AddSection(
            "centre-horizon-auditorium-mezzanine",
            "Mezzanine",
            auditorium,
            rowNames: ["D", "E"],
            seatsPerRow: 8);

        var conferenceRoom = AddVenueSpace(
            "centre-horizon-conference-room",
            "Salle de conférence",
            conventionCentre);
        AddSection(
            "centre-horizon-conference-room-standard",
            "Standard",
            conferenceRoom,
            rowNames: ["A", "B", "C"],
            seatsPerRow: 6);

        var venueTimeZone = TimeZoneInfo.FindSystemTimeZoneById(VenueTimeZoneId);
        var currentVenueDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(currentTime, venueTimeZone).DateTime);

        var concert = AddEvent(
            "nuit-electrique",
            "Nuit Électrique",
            "Une soirée de musique électronique réunissant plusieurs artistes canadiens.",
            AtVenueTime(30, 20),
            AtVenueTime(30, 23),
            "/images/events/nuit-electrique.webp",
            EventCategory.Concert,
            arenaMainFloor,
            EventStatus.Published);
        AddPrice(concert, lowerBowl, 79.00m);
        AddPrice(concert, upperBowl, 49.00m);

        var sportsEvent = AddEvent(
            "match-des-etoiles",
            "Match des Étoiles",
            "Un match amical opposant des joueurs invités de partout au Québec.",
            AtVenueTime(45, 19),
            AtVenueTime(45, 22),
            "/images/events/match-des-etoiles.webp",
            EventCategory.Sports,
            arenaMainFloor,
            EventStatus.Published);
        AddPrice(sportsEvent, lowerBowl, 65.00m);
        AddPrice(sportsEvent, upperBowl, 35.00m);

        var play = AddEvent(
            "les-heritiers-du-temps",
            "Les Héritiers du Temps",
            "Une création théâtrale originale sur la mémoire et les liens familiaux.",
            AtVenueTime(60, 19, 30),
            AtVenueTime(60, 22),
            "/images/events/les-heritiers-du-temps.webp",
            EventCategory.Theatre,
            theatreMainHall,
            EventStatus.Published);
        AddPrice(play, orchestra, 59.00m);
        AddPrice(play, balcony, 39.00m);

        var comedyShow = AddEvent(
            "rires-en-ville",
            "Rires en Ville",
            "Une soirée d'humour mettant en vedette des talents émergents.",
            AtVenueTime(75, 20),
            AtVenueTime(75, 22),
            "/images/events/rires-en-ville.webp",
            EventCategory.Comedy,
            theatreMainHall,
            EventStatus.Draft);
        AddPrice(comedyShow, orchestra, 45.00m);
        AddPrice(comedyShow, balcony, 30.00m);

        var conference = AddEvent(
            "sommet-innovation",
            "Sommet Innovation",
            "Une journée de conférences consacrée aux technologies et aux produits numériques.",
            AtVenueTime(90, 8, 30),
            AtVenueTime(90, 17),
            "/images/events/sommet-innovation.webp",
            EventCategory.Conference,
            auditorium,
            EventStatus.Cancelled);
        AddPrice(conference, auditoriumFloor, 120.00m);
        AddPrice(conference, mezzanine, 85.00m);

        return new SeedData(
            venues,
            venueSpaces,
            sections,
            rows,
            seats,
            events,
            eventSectionPrices);

        DateTimeOffset AtVenueTime(
            int daysFromNow,
            int hour,
            int minute = 0)
        {
            var localDateTime = currentVenueDate
                .AddDays(daysFromNow)
                .ToDateTime(
                    new TimeOnly(hour, minute),
                    DateTimeKind.Unspecified);

            return new DateTimeOffset(
                localDateTime,
                venueTimeZone.GetUtcOffset(localDateTime));
        }

        Venue AddVenue(string key, string name, string address, string city)
        {
            var venue = new Venue(CreateId($"venue:{key}"), name, address, city);
            venues.Add(venue);
            return venue;
        }

        VenueSpace AddVenueSpace(string key, string name, Venue venue)
        {
            var venueSpace = new VenueSpace(
                CreateId($"venue-space:{key}"),
                name,
                venue.Id);
            venueSpaces.Add(venueSpace);
            return venueSpace;
        }

        Section AddSection(
            string key,
            string name,
            VenueSpace venueSpace,
            IReadOnlyCollection<string> rowNames,
            int seatsPerRow)
        {
            var section = new Section(
                CreateId($"section:{key}"),
                name,
                venueSpace.Id);
            sections.Add(section);

            foreach (var rowName in rowNames)
            {
                var row = new Row(
                    CreateId($"row:{key}:{rowName}"),
                    rowName,
                    section.Id);
                rows.Add(row);

                for (var seatNumber = 1; seatNumber <= seatsPerRow; seatNumber++)
                {
                    seats.Add(new Seat(
                        CreateId($"seat:{key}:{rowName}:{seatNumber}"),
                        seatNumber.ToString(),
                        row.Id));
                }
            }

            return section;
        }

        Event AddEvent(
            string key,
            string name,
            string description,
            DateTimeOffset startsAt,
            DateTimeOffset endsAt,
            string imageUrl,
            EventCategory category,
            VenueSpace venueSpace,
            EventStatus status)
        {
            var @event = new Event(
                CreateId($"event:{key}"),
                name,
                description,
                startsAt,
                endsAt,
                imageUrl,
                category,
                venueSpace.Id);

            if (status == EventStatus.Published)
            {
                @event.Publish();
            }
            else if (status == EventStatus.Cancelled)
            {
                @event.Cancel();
            }

            events.Add(@event);
            return @event;
        }

        void AddPrice(Event @event, Section section, decimal amount)
        {
            eventSectionPrices.Add(new EventSectionPrice(
                CreateId($"event-section-price:{@event.Id}:{section.Id}"),
                @event.Id,
                section.Id,
                amount,
                Currency));
        }
    }

    private static void ValidatePricingConsistency(SeedData seedData)
    {
        var eventsById = seedData.Events.ToDictionary(@event => @event.Id);
        var sectionsById = seedData.Sections.ToDictionary(section => section.Id);

        foreach (var price in seedData.EventSectionPrices)
        {
            var @event = eventsById[price.EventId];
            var section = sectionsById[price.SectionId];

            if (@event.VenueSpaceId != section.VenueSpaceId)
            {
                throw new InvalidOperationException(
                    $"Section '{section.Id}' does not belong to the venue space " +
                    $"of event '{@event.Id}'.");
            }
        }
    }

    private static Guid CreateId(string key)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes($"{IdNamespace}:{key}"));

        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed record SeedData(
        IReadOnlyCollection<Venue> Venues,
        IReadOnlyCollection<VenueSpace> VenueSpaces,
        IReadOnlyCollection<Section> Sections,
        IReadOnlyCollection<Row> Rows,
        IReadOnlyCollection<Seat> Seats,
        IReadOnlyCollection<Event> Events,
        IReadOnlyCollection<EventSectionPrice> EventSectionPrices);
}
