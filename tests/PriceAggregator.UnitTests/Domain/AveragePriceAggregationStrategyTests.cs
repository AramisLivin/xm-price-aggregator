using PriceAggregator.Domain;
using PriceAggregator.Domain.Aggregation;

namespace PriceAggregator.UnitTests.Domain;

public class AveragePriceAggregationStrategyTests
{
    private readonly AveragePriceAggregationStrategy _strategy = new();

    [Fact]
    public void Aggregate_ReturnsArithmeticMean()
    {
        var prices = new[] { new SourcePrice("A", 16531.00m), new SourcePrice("B", 16529.50m) };

        Assert.Equal(16530.25m, _strategy.Aggregate(prices));
    }

    [Fact]
    public void Aggregate_SinglePrice_ReturnsIt()
    {
        Assert.Equal(100m, _strategy.Aggregate([new SourcePrice("A", 100m)]));
    }

    [Fact]
    public void Aggregate_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => _strategy.Aggregate([]));
    }
}
