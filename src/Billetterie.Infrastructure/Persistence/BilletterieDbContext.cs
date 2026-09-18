using Microsoft.EntityFrameworkCore;

namespace Billetterie.Infrastructure.Persistence;

public class BilletterieDbContext : DbContext
{
    public BilletterieDbContext(
        DbContextOptions<BilletterieDbContext> options)
        : base(options)
    {
    }
}