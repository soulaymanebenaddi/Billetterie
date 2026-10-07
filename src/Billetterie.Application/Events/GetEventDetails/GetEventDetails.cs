namespace Billetterie.Application.Events.GetEventDetails;

public sealed class GetEventDetails
{
    private readonly IEventDetailsQuery _eventDetailsQuery;


    public GetEventDetails(IEventDetailsQuery eventDetailsQuery)
    {
        ArgumentNullException.ThrowIfNull(eventDetailsQuery);

        _eventDetailsQuery = eventDetailsQuery;
    }

    public Task<EventDetailsDto?> ExecuteAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return _eventDetailsQuery.GetEventDetailsAsync(eventId, cancellationToken);
    }
}