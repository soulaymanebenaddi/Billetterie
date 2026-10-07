using Billetterie.Application.Events.GetEventDetails;
using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Billetterie.Infrastructure.Persistence.Queries;

public sealed class EventDetailsQuery : IEventDetailsQuery
{
    private readonly BilletterieDbContext _dbContext;

    public EventDetailsQuery(BilletterieDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var query =
            from @event in _dbContext.Events.AsNoTracking()
            where @event.Id == eventId
            where @event.Status == EventStatus.Published
            join venueSpace in _dbContext.VenueSpaces
                on @event.VenueSpaceId equals venueSpace.Id
            join venue in _dbContext.Venues
                on venueSpace.VenueId equals venue.Id
            from price in _dbContext.EventSectionPrices
                .Where(price => price.EventId == @event.Id)
                .OrderBy(price => price.Amount)
                .ThenBy(price => price.Id)
                .Select(price => new
                {
                    Amount = (decimal?)price.Amount,
                    Currency = (string?)price.Currency
                })
                .Take(1)
                .DefaultIfEmpty()
            select new EventDetailsDto(
                @event.Id,
                @event.Name,
                @event.Description,
                @event.ImageUrl,
                @event.Category,
                @event.StartsAt,
                @event.EndsAt,
                venue.Name,
                venue.Address,
                venue.City,
                venueSpace.Name,
                price.Amount,
                price.Currency);

        return await query.SingleOrDefaultAsync(cancellationToken);
    }
}