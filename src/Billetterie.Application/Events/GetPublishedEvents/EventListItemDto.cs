namespace Billetterie.Application.Events.GetPublishedEvents;

/// <summary>
/// Event information displayed on a catalog card.
/// </summary>
/// <param name="StartingPrice">The lowest section price, or null when no price is configured.</param>
/// <param name="Currency">The currency of the starting price, or null when no price is configured.</param>
public sealed record EventListItemDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    EventCategory Category,
    DateTimeOffset StartsAt,
    string VenueName,
    string City,
    decimal? StartingPrice,
    string? Currency);
