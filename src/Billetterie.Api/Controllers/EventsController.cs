using Billetterie.Application.Events.GetPublishedEvents;
using Billetterie.Application.Events.GetEventDetails;
using Billetterie.Application.Events.GetEventSeats;
using Microsoft.AspNetCore.Mvc;

namespace Billetterie.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly GetPublishedEvents _getPublishedEvents;

    private readonly GetEventDetails _getEventDetails;

    private readonly GetEventSeats _getEventSeats;

    public EventsController(GetPublishedEvents getPublishedEvents, GetEventDetails getEventDetails, GetEventSeats getEventSeats)
    {
        ArgumentNullException.ThrowIfNull(getPublishedEvents);
        ArgumentNullException.ThrowIfNull(getEventDetails);
        ArgumentNullException.ThrowIfNull(getEventSeats);

        _getPublishedEvents = getPublishedEvents;
        _getEventDetails = getEventDetails;
        _getEventSeats = getEventSeats;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDetailsDto>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var eventDetails = await _getEventDetails.ExecuteAsync(id, cancellationToken);

        if (eventDetails is null)
        {
            return NotFound();
        }

        return Ok(eventDetails);
    }

    [HttpGet("{id}/seats")]
    [ProducesResponseType(typeof(IReadOnlyList<EventSeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EventSeatDto>>> GetSeats(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var eventSeats = await _getEventSeats.ExecuteAsync(id, cancellationToken);

        if( eventSeats is null)
        {
            return NotFound();
        }

        return Ok(eventSeats);
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
