namespace Billetterie.Application.Events.GetEventSeats;

public sealed record EventSeatDto(
    Guid Id,
    string Label,
    Guid RowId,
    string RowName,
    Guid SectionId,
    string SectionName,
    decimal? Price,
    string? Currency,
    bool IsAvailable
);