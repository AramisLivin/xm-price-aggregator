namespace PriceAggregator.Domain.Aggregation;

public sealed class AveragePriceAggregationStrategy : IPriceAggregationStrategy
{
    public double Aggregate(IReadOnlyCollection<SourcePrice> prices)
    {
        ArgumentNullException.ThrowIfNull(prices);
        if (prices.Count == 0)
        {
            throw new ArgumentException("Cannot aggregate an empty set", nameof(prices));
        }

        return prices.Average(p => p.Price);
    }
}
