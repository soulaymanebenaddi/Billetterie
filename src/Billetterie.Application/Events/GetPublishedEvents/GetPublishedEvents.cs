namespace Billetterie.Application.Events.GetPublishedEvents;

public sealed class GetPublishedEvents
{
    private readonly IEventCatalogQuery _eventCatalogQuery;
    private readonly TimeProvider _timeProvider;

    public GetPublishedEvents(
        IEventCatalogQuery eventCatalogQuery,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(eventCatalogQuery);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _eventCatalogQuery = eventCatalogQuery;
        _timeProvider = timeProvider;
    }

    public Task<IReadOnlyList<EventListItemDto>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var currentTime = _timeProvider.GetUtcNow();

        return _eventCatalogQuery.GetPublishedEventsAsync(currentTime, cancellationToken);
    }
}
