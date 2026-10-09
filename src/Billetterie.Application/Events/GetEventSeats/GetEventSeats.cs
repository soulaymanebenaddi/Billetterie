namespace Billetterie.Application.Events.GetEventSeats;

public sealed class GetEventSeats
{
    private readonly IEventSeatQuery _eventSeatQuery;

    public GetEventSeats(IEventSeatQuery eventSeatQuery)
    {
        ArgumentNullException.ThrowIfNull(eventSeatQuery);

        _eventSeatQuery = eventSeatQuery;
    }

    public Task<IReadOnlyList<EventSeatDto>?> ExecuteAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return _eventSeatQuery.GetEventSeatsAsync(eventId, cancellationToken);
    }
}