using PriceAggregator.Domain;

namespace PriceAggregator.Infrastructure.Persistence;

internal static class PriceRecordMapping
{
    public static AggregatedPriceRecord ToRecord(this AggregatedPrice price)
    {
        var record = new AggregatedPriceRecord
        {
            Instrument = price.Instrument.Symbol,
            Timestamp = price.Timestamp.ToUnixSeconds(),
            Price = price.Price,
        };

        foreach (var source in price.Sources)
        {
            record.Sources.Add(new SourcePriceRecord { Source = source.Source, Price = source.Price });
        }

        return record;
    }

    public static AggregatedPrice ToDomain(this AggregatedPriceRecord record) =>
        new(
            Instrument.Parse(record.Instrument),
            HourlyTimestamp.FromUnixSeconds(record.Timestamp),
            record.Price,
            record.Sources.Select(s => new SourcePrice(s.Source, s.Price)).ToList());
}
