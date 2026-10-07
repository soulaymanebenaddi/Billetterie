namespace Billetterie.Application.Events.GetEventDetails;

public sealed record EventDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    EventCategory Category,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string VenueName,
    string Address,
    string City,
    string VenueSpaceName,
    decimal? StartingPrice,
    string? Currency
);