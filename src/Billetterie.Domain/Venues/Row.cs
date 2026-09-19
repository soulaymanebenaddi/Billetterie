namespace Billetterie.Domain.Venues;

public class Row
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Guid SectionId { get; private set; }

    public Row(Guid id, string name, Guid sectionId)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if(id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if(sectionId == Guid.Empty)
        {
            throw new ArgumentException("SectionId cannot be empty.", nameof(sectionId));
        }

        Id = id;
        Name = name;
        SectionId = sectionId;
    }
}