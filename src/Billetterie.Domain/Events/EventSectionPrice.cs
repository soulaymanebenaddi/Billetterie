namespace Billetterie.Domain.Events;

public class EventSectionPrice
{
    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid SectionId { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public const int AmountPrecision = 18;

    public const int AmountScale = 2;

    public const decimal MaxAmount = 9_999_999_999_999_999.99m;

    public const int MaxCurrencyLength = 3;

    public EventSectionPrice(Guid id, Guid eventId, Guid sectionId, decimal amount, string currency)
    {

        if (amount < 0)
            throw new ArgumentException(
                "Price amount cannot be negative.",
                nameof(amount));

        if (decimal.Round(amount, AmountScale) != amount)
            throw new ArgumentException(
                $"Price amount cannot have more than {AmountScale} decimal places.",
                nameof(amount));

        if (amount > MaxAmount)
            throw new ArgumentException(
                $"Price amount cannot exceed {MaxAmount}.",
                nameof(amount));

        if (id == Guid.Empty)
            throw new ArgumentException(
                "Price ID cannot be empty.",
                nameof(id));

        if (sectionId == Guid.Empty)
            throw new ArgumentException(
                "Section ID cannot be empty.",
                nameof(sectionId));
        
        if (eventId == Guid.Empty)
            throw new ArgumentException(
                "Event ID cannot be empty.",
                nameof(eventId));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException(
                "Currency cannot be empty.",
                nameof(currency));

        if (currency.Length != MaxCurrencyLength)
            throw new ArgumentException(
                $"Currency must be exactly {MaxCurrencyLength} characters long.",
                nameof(currency));


        Id = id;
        EventId = eventId;
        SectionId = sectionId;
        Amount = amount;
        Currency = currency;
    }
}