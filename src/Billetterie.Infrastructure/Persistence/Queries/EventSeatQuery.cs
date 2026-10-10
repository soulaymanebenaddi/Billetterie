using Billetterie.Application.Events.GetEventSeats;
using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Billetterie.Infrastructure.Persistence.Queries;

public sealed class EventSeatQuery : IEventSeatQuery
{
    private readonly BilletterieDbContext _dbContext;

    public EventSeatQuery(BilletterieDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventSeatDto>?> GetEventSeatsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var eventExists = await _dbContext.Events
            .AsNoTracking()
            .AnyAsync(
                @event => @event.Id == eventId && @event.Status == EventStatus.Published,
                cancellationToken);

        if (!eventExists)
        {
            return null;
        }

        var query =
            from @event in _dbContext.Events.AsNoTracking()
            where @event.Id == eventId
            where @event.Status == EventStatus.Published
            join venueSpace in _dbContext.VenueSpaces
                on @event.VenueSpaceId equals venueSpace.Id
            join section in _dbContext.Sections
                on venueSpace.Id equals section.VenueSpaceId
            join row in _dbContext.Rows
                on section.Id equals row.SectionId
            join seat in _dbContext.Seats
                on row.Id equals seat.RowId
            join price in _dbContext.EventSectionPrices
                on new { EventId = @event.Id, SectionId = section.Id }
                equals new { price.EventId, price.SectionId }
                into priceGroup
            from price in priceGroup.DefaultIfEmpty()
            orderby section.Name, section.Id, row.Name, row.Id, seat.Label, seat.Id
            select new EventSeatDto(
                seat.Id,
                seat.Label,
                row.Id,
                row.Name,
                section.Id,
                section.Name,
                price.Amount,
                price.Currency,
                true); // Assuming all seats are available, we will adjust this after finishing reservations

        return await query.ToListAsync(cancellationToken);
    }
}
