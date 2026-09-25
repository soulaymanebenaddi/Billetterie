using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.RowId, s.Label })
            .IsUnique();

        builder.Property(s => s.Label)
            .IsRequired()
            .HasMaxLength(Seat.MaxLabelLength);

        builder.HasOne<Row>()
            .WithMany()
            .HasForeignKey(s => s.RowId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
