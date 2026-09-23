using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.StartsAt)
            .IsRequired();

        builder.Property(e => e.EndsAt)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.ImageUrl)
            .IsRequired(false);

        builder.Property(e => e.Category)
            .IsRequired();

        builder.Property(e => e.Description)
            .IsRequired(false)
            .HasMaxLength(5000);

        builder.HasOne<VenueSpace>()
            .WithMany()
            .HasForeignKey(e => e.VenueSpaceId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
