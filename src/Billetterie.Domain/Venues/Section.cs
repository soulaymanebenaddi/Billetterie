namespace Billetterie.Domain.Venues;

public class Section
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid VenueSpaceId { get; private set; }

    public const int MaxNameLength = 100;

    public Section(Guid id, string name, Guid venueSpaceId)
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

        if(venueSpaceId == Guid.Empty)
        {
            throw new ArgumentException("VenueSpaceId cannot be empty.", nameof(venueSpaceId));
        }

        Id = id;
        Name = name;
        VenueSpaceId = venueSpaceId;
    }
}