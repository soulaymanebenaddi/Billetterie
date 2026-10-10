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
    Task<IReadOnlyList<EventSeatDto>?> GetEventSeatsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}
