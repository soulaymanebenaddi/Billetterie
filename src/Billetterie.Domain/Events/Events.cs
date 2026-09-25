namespace Billetterie.Domain.Events;

public class Event
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public EventStatus Status { get; private set; }

    public string? ImageUrl { get; private set; }

    public EventCategory Category { get; private set; }

    public Guid VenueSpaceId { get; private set; }

    public const int MaxNameLength = 200;

    public const int MaxDescriptionLength = 5000;

    public Event(
        Guid id,
        string name,
        string? description,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string? imageUrl,
        EventCategory category,
        Guid venueSpaceId)
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Event name cannot be empty.",
                nameof(name));

        if (!Enum.IsDefined(category))
            throw new ArgumentException(
                "Event category is invalid.",
                nameof(category));

        if (name.Length > MaxNameLength)
            throw new ArgumentException(
                $"Event name cannot exceed {MaxNameLength} characters.",
                nameof(name));

        if (description != null && description.Length > MaxDescriptionLength)
            throw new ArgumentException(
                $"Event description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));

        if (endsAt <= startsAt)
            throw new ArgumentException(
                "Event end date must be after its start date.");

        if (id == Guid.Empty)
            throw new ArgumentException(
                "Event ID cannot be empty.",
                nameof(id));

        if (venueSpaceId == Guid.Empty)
            throw new ArgumentException(
                "Venue space ID cannot be empty.",
                nameof(venueSpaceId));

        Id = id;
        Name = name;
        Description = description;
        StartsAt = startsAt.ToUniversalTime();
        EndsAt = endsAt.ToUniversalTime();
        Status = EventStatus.Draft;
        ImageUrl = imageUrl;
        Category = category;
        VenueSpaceId = venueSpaceId;
    }

    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException(
                "Only draft events can be published.");

        Status = EventStatus.Published;
    }

    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException(
                "Event is already cancelled.");

        Status = EventStatus.Cancelled;
    }
}