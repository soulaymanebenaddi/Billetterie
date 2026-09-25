using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.VenueSpaceId, s.Name })
            .IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(Section.MaxNameLength);

        builder.HasOne<VenueSpace>()
            .WithMany()
            .HasForeignKey(s => s.VenueSpaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
