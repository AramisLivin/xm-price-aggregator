namespace PriceAggregator.Domain;

public sealed class AggregatedPrice
{
    public AggregatedPrice(Instrument instrument, HourlyTimestamp timestamp, double price, IReadOnlyList<SourcePrice> sources)
    {
        ArgumentNullException.ThrowIfNull(instrument);
        ArgumentNullException.ThrowIfNull(sources);
        if (sources.Count == 0)
        {
            throw new ArgumentException("At least one price is required", nameof(sources));
        }

        if (!double.IsFinite(price) || price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be positive and finite");
        }

        Instrument = instrument;
        Timestamp = timestamp;
        Price = price;
        Sources = sources.ToArray();
    }

    public Instrument Instrument { get; }

    public HourlyTimestamp Timestamp { get; }

    public double Price { get; }

    public IReadOnlyList<SourcePrice> Sources { get; }
}
