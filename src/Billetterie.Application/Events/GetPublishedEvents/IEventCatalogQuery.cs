namespace Billetterie.Application.Events.GetPublishedEvents;

public interface IEventCatalogQuery
{
    /// <summary>
    /// Returns published events starting strictly after <paramref name="currentTime"/>,
    /// ordered by start date in ascending order.
    /// </summary>
    /// <returns>An empty list when no events match.</returns>
    Task<IReadOnlyList<EventListItemDto>> GetPublishedEventsAsync(
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default);
}
