namespace PriceAggregator.Infrastructure.Persistence;

internal sealed class AggregatedPriceRecord
{
    public required string Instrument { get; init; }

    public long Timestamp { get; init; }

    public decimal Price { get; init; }

    public ICollection<SourcePriceRecord> Sources { get; } = new List<SourcePriceRecord>();
}

internal sealed class SourcePriceRecord
{
    public required string Source { get; init; }

    public decimal Price { get; init; }
}
