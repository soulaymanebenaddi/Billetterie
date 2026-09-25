using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(Venue.MaxNameLength);

        builder.Property(v => v.Address)
            .IsRequired()
            .HasMaxLength(Venue.MaxAddressLength);

        builder.Property(v => v.City)
            .IsRequired()
            .HasMaxLength(Venue.MaxCityLength);
    }
}
