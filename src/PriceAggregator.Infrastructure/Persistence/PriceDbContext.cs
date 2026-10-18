using Microsoft.EntityFrameworkCore;

namespace PriceAggregator.Infrastructure.Persistence;

internal sealed class PriceDbContext(DbContextOptions<PriceDbContext> options) : DbContext(options)
{
    public DbSet<AggregatedPriceRecord> AggregatedPrices => Set<AggregatedPriceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PriceDbContext).Assembly);
}
