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

    public BilletterieDbContext(
        DbContextOptions<BilletterieDbContext> options)
        : base(options)
    {
    }
}
