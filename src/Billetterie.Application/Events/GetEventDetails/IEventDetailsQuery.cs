namespace Billetterie.Application.Events.GetEventDetails;

public interface IEventDetailsQuery
{
    /// <summary>
    /// Returns the details of an event with the given <paramref name="eventId"/>.
    /// </summary>
    /// <returns><c>null</c> when no event matches.</returns>
    Task<EventDetailsDto?> GetEventDetailsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}