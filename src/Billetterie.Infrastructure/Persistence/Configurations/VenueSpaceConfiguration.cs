using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class VenueSpaceConfiguration : IEntityTypeConfiguration<VenueSpace>
{
    public void Configure(EntityTypeBuilder<VenueSpace> builder)
    {
        builder.HasKey(v => v.Id);

        builder.HasIndex(v => new { v.VenueId, v.Name })
            .IsUnique();

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne<Venue>()
            .WithMany()
            .HasForeignKey(v => v.VenueId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
