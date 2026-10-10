namespace Billetterie.Application.Events.GetEventSeats;

public interface IEventSeatQuery
{
    /// <summary>
    /// Returns the seats of the published event with the given <paramref name="eventId"/>,
    /// regardless of its start or end date, including events that have already started or ended.
    /// </summary>
    /// <returns>
    /// The published event's seats, ordered by section, row and seat label, with identifiers
    /// breaking ties. Returns null when the identifier is unknown or the event is draft or
    /// cancelled, or an empty collection when the published event has no seats.
    /// </returns>
    /// <remarks>
    /// Publication and seats are read together in one SQL statement, so null versus
    /// an empty collection is determined from the same database snapshot.
    /// Seats without a section price for this event have null Price and Currency;
    /// a configured free price remains zero. IsAvailable is temporarily always true
    /// until reservations and purchases are implemented, and does not guarantee
    /// that a future reservation will succeed or that the event is open for sale.
    /// </remarks>
    Task<IReadOnlyList<EventSeatDto>?> GetEventSeatsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}
