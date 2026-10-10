using Billetterie.Application.Events.GetEventSeats;
using Billetterie.Domain.Events;
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
        var query =
            from @event in _dbContext.Events.AsNoTracking().AsSingleQuery()
            where @event.Id == eventId
            where @event.Status == EventStatus.Published
            select new
            {
                // Conserve l'événement même si sa salle n'a aucun siège.
                // La publication et les sièges sont lus dans le même snapshot SQL.
                Seats = (
                    from section in _dbContext.Sections
                    where section.VenueSpaceId == @event.VenueSpaceId
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
                        true) // Disponibilité provisoire jusqu'aux réservations et achats.
                ).ToList()
            };

        var result = await query.SingleOrDefaultAsync(cancellationToken);
        return result?.Seats;
    }
}
