using Billetterie.Application.Events.GetPublishedEvents;
using Billetterie.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Billetterie.Infrastructure.Persistence.Queries;

public sealed class EventCatalogQuery : IEventCatalogQuery
{
    private readonly BilletterieDbContext _dbContext;

    public EventCatalogQuery(BilletterieDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventListItemDto>> GetPublishedEventsAsync(
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default)
    {
        var utcCurrentTime = currentTime.ToUniversalTime();

        var query =
            from @event in _dbContext.Events.AsNoTracking()
            where @event.Status == EventStatus.Published
                && @event.StartsAt > utcCurrentTime
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
            orderby @event.StartsAt, @event.Id
            select new EventListItemDto(
                @event.Id,
                @event.Name,
                @event.ImageUrl,
                @event.Category,
                @event.StartsAt,
                venue.Name,
                venue.City,
                price.Amount,
                price.Currency);

        return await query.ToListAsync(cancellationToken);
    }
}
