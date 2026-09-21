using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class RowConfiguration : IEntityTypeConfiguration<Row>
{
    public void Configure(EntityTypeBuilder<Row> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.SectionId, r.Name })
            .IsUnique();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne<Section>()
            .WithMany()
            .HasForeignKey(r => r.SectionId)
            .IsRequired();
    }
}
