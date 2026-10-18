using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PriceAggregator.Infrastructure.Persistence;

internal sealed class AggregatedPriceRecordConfiguration : IEntityTypeConfiguration<AggregatedPriceRecord>
{
    public void Configure(EntityTypeBuilder<AggregatedPriceRecord> builder)
    {
        builder.ToTable("AggregatedPrices");
        builder.HasKey(p => new { p.Instrument, p.Timestamp });
        builder.Property(p => p.Instrument).HasMaxLength(32);
        builder.OwnsMany(p => p.Sources, sources =>
        {
            sources.ToJson();
            sources.Property(s => s.Source).HasMaxLength(64);
        });
    }
}
