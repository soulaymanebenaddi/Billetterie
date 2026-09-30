using Billetterie.Application.Events.GetPublishedEvents;

namespace Billetterie.UnitTests.Events;

public class GetPublishedEventsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_ForwardsCurrentTimeAndCancellationToken_AndReturnsQueryResults(
        bool hasEvents)
    {
        var currentTime = new DateTimeOffset(2030, 6, 15, 12, 0, 0, TimeSpan.Zero);
        IReadOnlyList<EventListItemDto> expectedEvents = hasEvents
            ? [new EventListItemDto(
                Guid.NewGuid(),
                "Concert",
                "/images/events/concert.webp",
                EventCategory.Concert,
                currentTime.AddDays(1),
                "Théâtre Maisonneuve",
                "Montréal",
                49m,
                "CAD")]
            : [];
        var query = new StubEventCatalogQuery(expectedEvents);
        var timeProvider = new FixedTimeProvider(currentTime);
        var useCase = new GetPublishedEvents(query, timeProvider);
        using var cancellationTokenSource = new CancellationTokenSource();

        var result = await useCase.ExecuteAsync(cancellationTokenSource.Token);

        Assert.Equal(currentTime, query.ReceivedCurrentTime);
        Assert.Equal(cancellationTokenSource.Token, query.ReceivedCancellationToken);
        Assert.Equal(expectedEvents, result);
    }

    private sealed class StubEventCatalogQuery : IEventCatalogQuery
    {
        private readonly IReadOnlyList<EventListItemDto> _events;

        public DateTimeOffset? ReceivedCurrentTime { get; private set; }

        public CancellationToken? ReceivedCancellationToken { get; private set; }

        public StubEventCatalogQuery(IReadOnlyList<EventListItemDto> events)
        {
            _events = events;
        }

        public Task<IReadOnlyList<EventListItemDto>> GetPublishedEventsAsync(
            DateTimeOffset currentTime,
            CancellationToken cancellationToken = default)
        {
            ReceivedCurrentTime = currentTime;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(_events);
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _currentTime;

        public FixedTimeProvider(DateTimeOffset currentTime)
        {
            _currentTime = currentTime;
        }

        public override DateTimeOffset GetUtcNow() => _currentTime;
    }
}
