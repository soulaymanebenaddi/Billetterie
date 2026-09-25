using Billetterie.Domain.Events;
using Billetterie.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Billetterie.Infrastructure.Persistence;

public class BilletterieDbContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<VenueSpace> VenueSpaces => Set<VenueSpace>();

    public DbSet<Section> Sections => Set<Section>();

    public DbSet<Row> Rows => Set<Row>();

    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<EventSectionPrice> EventSectionPrices => Set<EventSectionPrice>();

    public BilletterieDbContext(
        DbContextOptions<BilletterieDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BilletterieDbContext).Assembly);
    }
}
