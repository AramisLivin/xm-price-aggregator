namespace PriceAggregator.Domain;

public sealed class AggregatedPrice
{
    public AggregatedPrice(Instrument instrument, HourlyTimestamp timestamp, decimal price, IReadOnlyList<SourcePrice> sources)
    {
        ArgumentNullException.ThrowIfNull(instrument);
        ArgumentNullException.ThrowIfNull(sources);
        if (sources.Count == 0)
        {
            throw new ArgumentException("At least one price is required", nameof(sources));
        }

        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be positive");
        }

        Instrument = instrument;
        Timestamp = timestamp;
        Price = price;
        Sources = sources.ToArray();
    }

    public Instrument Instrument { get; }

    public HourlyTimestamp Timestamp { get; }

    public decimal Price { get; }

    public IReadOnlyList<SourcePrice> Sources { get; }
}
