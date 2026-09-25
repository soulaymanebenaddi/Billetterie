using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class EventSectionPriceConfiguration : IEntityTypeConfiguration<EventSectionPrice>
{
    public void Configure(EntityTypeBuilder<EventSectionPrice> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.EventId, e.SectionId })
            .IsUnique();

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne<Section>()
            .WithMany()
            .HasForeignKey(e => e.SectionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(18, 2); 

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(EventSectionPrice.MaxCurrencyLength);
    }
}