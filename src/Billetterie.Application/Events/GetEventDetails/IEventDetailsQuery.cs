namespace Billetterie.Application.Events.GetEventDetails;

public interface IEventDetailsQuery
{
    /// <summary>
    /// Returns the public details of the published event with the given <paramref name="eventId"/>,
    /// regardless of its start or end date, including events that have already started or ended.
    /// </summary>
    /// <returns>
    /// The published event's details, or <c>null</c> when the identifier is unknown
    /// or the event is draft or cancelled.
    /// </returns>
    Task<EventDetailsDto?> GetEventDetailsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}
