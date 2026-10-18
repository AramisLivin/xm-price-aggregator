using PriceAggregator.Application.Abstractions;
using PriceAggregator.Domain;

namespace PriceAggregator.UnitTests.Fakes;

internal sealed class FakePriceSource(string name, Func<decimal?> behavior) : IPriceSource
{
    public string Name { get; } = name;

    public int Calls { get; private set; }

    public static FakePriceSource Returning(string name, decimal? price) => new(name, () => price);

    public static FakePriceSource Failing(string name) =>
        new(name, () => throw new HttpRequestException($"{name} is down"));

    public Task<decimal?> GetClosePriceAsync(Instrument instrument, HourlyTimestamp timestamp, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(behavior());
    }
}
