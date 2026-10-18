namespace PriceAggregator.Domain.Aggregation;

public interface IPriceAggregationStrategy
{
    double Aggregate(IReadOnlyCollection<SourcePrice> prices);
}
