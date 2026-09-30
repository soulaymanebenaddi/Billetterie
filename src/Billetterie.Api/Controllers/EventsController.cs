using Billetterie.Application.Events.GetPublishedEvents;
using Microsoft.AspNetCore.Mvc;

namespace Billetterie.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly GetPublishedEvents _getPublishedEvents;

    public EventsController(GetPublishedEvents getPublishedEvents)
    {
        ArgumentNullException.ThrowIfNull(getPublishedEvents);

        _getPublishedEvents = getPublishedEvents;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventListItemDto>>> Get(
        CancellationToken cancellationToken)
    {
        var events = await _getPublishedEvents.ExecuteAsync(cancellationToken);

        return Ok(events);
    }
}
