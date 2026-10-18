namespace PriceAggregator.Domain.Aggregation;

public interface IPriceAggregationStrategy
{
    decimal Aggregate(IReadOnlyCollection<SourcePrice> prices);
}
