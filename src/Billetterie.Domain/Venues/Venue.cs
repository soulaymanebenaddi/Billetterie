namespace Billetterie.Domain.Venues;

public class Venue
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }

    public const int MaxNameLength = 200;
    public const int MaxAddressLength = 300;
    public const int MaxCityLength = 100;

    public Venue(Guid id, string name, string address, string city)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if(string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));
        }

        if(string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City cannot be null or empty.", nameof(city));
        }

        if(name.Length > MaxNameLength)
        {
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        if(address.Length > MaxAddressLength)
        {
            throw new ArgumentException($"Address cannot exceed {MaxAddressLength} characters.", nameof(address));
        }

        if(city.Length > MaxCityLength)
        {
            throw new ArgumentException($"City cannot exceed {MaxCityLength} characters.", nameof(city));
        }

        if(id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        Id = id;
        Name = name;
        Address = address;
        City = city;
    }
}