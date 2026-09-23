namespace Billetterie.Domain.Venues;

public class VenueSpace
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid VenueId { get; private set; }

    public const int MaxNameLength = 200;

    public VenueSpace(Guid id, string name, Guid venueId)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if(name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        if(id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if(venueId == Guid.Empty)
        {
            throw new ArgumentException("VenueId cannot be empty.", nameof(venueId));
        }

        Id = id;
        Name = name;
        VenueId = venueId;
    }
}