using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;

namespace Billetterie.UnitTests.Events;

public class EventSectionPriceTests
{
    public static TheoryData<decimal> ValidAmounts => new()
    {
        0m,
        0.01m,
        49.99m,
        49.990m,
        9_999_999_999_999_999.99m
    };

    public static TheoryData<decimal> InvalidAmounts => new()
    {
        -0.01m,
        49.991m,
        49.999m,
        10_000_000_000_000_000.00m
    };

    [Theory]
    [MemberData(nameof(ValidAmounts))]
    public void Create_WithValidInputs_CreatesPrice(decimal amount)
    {
        var id = Guid.NewGuid();
        var @event = CreateEvent();
        var section = new Section(Guid.NewGuid(), "Balcon", @event.VenueSpaceId);

        var price = EventSectionPrice.Create(id, @event, section, amount, "CAD");

        Assert.Equal(id, price.Id);
        Assert.Equal(@event.Id, price.EventId);
        Assert.Equal(section.Id, price.SectionId);
        Assert.Equal(amount, price.Amount);
        Assert.Equal("CAD", price.Currency);
    }

    [Fact]
    public void Create_WithSectionFromAnotherVenueSpace_ThrowsArgumentException()
    {
        var @event = CreateEvent();
        var section = new Section(Guid.NewGuid(), "Balcon", Guid.NewGuid());

        var exception = Assert.Throws<ArgumentException>(() =>
            EventSectionPrice.Create(Guid.NewGuid(), @event, section, 49m, "CAD"));

        Assert.Equal("section", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullEvent_ThrowsArgumentNullException()
    {
        var section = new Section(Guid.NewGuid(), "Balcon", Guid.NewGuid());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            EventSectionPrice.Create(Guid.NewGuid(), null!, section, 49m, "CAD"));

        Assert.Equal("@event", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullSection_ThrowsArgumentNullException()
    {
        var @event = CreateEvent();

        var exception = Assert.Throws<ArgumentNullException>(() =>
            EventSectionPrice.Create(Guid.NewGuid(), @event, null!, 49m, "CAD"));

        Assert.Equal("section", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsArgumentException()
    {
        var @event = CreateEvent();
        var section = new Section(Guid.NewGuid(), "Balcon", @event.VenueSpaceId);

        var exception = Assert.Throws<ArgumentException>(() =>
            EventSectionPrice.Create(Guid.Empty, @event, section, 49m, "CAD"));

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(InvalidAmounts))]
    public void Create_WithInvalidAmount_ThrowsArgumentException(decimal amount)
    {
        var @event = CreateEvent();
        var section = new Section(Guid.NewGuid(), "Balcon", @event.VenueSpaceId);

        var exception = Assert.Throws<ArgumentException>(() =>
            EventSectionPrice.Create(Guid.NewGuid(), @event, section, amount, "CAD"));

        Assert.Equal("amount", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("CA")]
    [InlineData("CADD")]
    public void Create_WithInvalidCurrency_ThrowsArgumentException(string? currency)
    {
        var @event = CreateEvent();
        var section = new Section(Guid.NewGuid(), "Balcon", @event.VenueSpaceId);

        var exception = Assert.Throws<ArgumentException>(() =>
            EventSectionPrice.Create(Guid.NewGuid(), @event, section, 49m, currency!));

        Assert.Equal("currency", exception.ParamName);
    }

    private static Event CreateEvent()
    {
        var startsAt = new DateTimeOffset(2030, 6, 15, 20, 0, 0, TimeSpan.Zero);

        return new Event(
            Guid.NewGuid(),
            "Concert",
            null,
            startsAt,
            startsAt.AddHours(2),
            null,
            EventCategory.Concert,
            Guid.NewGuid());
    }
}
