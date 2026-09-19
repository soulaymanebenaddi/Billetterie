namespace Billetterie.Domain.Events;

public class Event
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public EventStatus Status { get; private set; }

    public Event(
        Guid id,
        string name,
        string? description,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Event name cannot be empty.",
                nameof(name));

        if (endsAt <= startsAt)
            throw new ArgumentException(
                "Event end date must be after its start date.");

        Id = id;
        Name = name;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Status = EventStatus.Draft;
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