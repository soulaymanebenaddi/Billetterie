namespace Billetterie.Domain.Venues;

public class Seat
{
    public Guid Id { get; private set; }
    public string Label { get; private set; }
    public Guid RowId { get; private set; }

    public Seat(Guid id, string label, Guid rowId)
    {
        if(string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Label cannot be null or empty.", nameof(label));
        }

        if(id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if(rowId == Guid.Empty)
        {
            throw new ArgumentException("RowId cannot be empty.", nameof(rowId));
        }

        Id = id;
        Label = label;
        RowId = rowId;
    }
}