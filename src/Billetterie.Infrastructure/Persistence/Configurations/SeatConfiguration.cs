using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billetterie.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Label)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne<Row>()
            .WithMany()
            .HasForeignKey(s => s.RowId)
            .IsRequired();
    }
}
