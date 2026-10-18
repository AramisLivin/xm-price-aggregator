using PriceAggregator.Application.Abstractions;
using PriceAggregator.Domain;

namespace PriceAggregator.IntegrationTests;

public sealed class ConfigurablePriceSource(string name, decimal price) : IPriceSource
{
    public string Name { get; } = name;

    public bool IsDown { get; set; }

    public Task<decimal?> GetClosePriceAsync(Instrument instrument, HourlyTimestamp timestamp, CancellationToken cancellationToken) =>
        IsDown
            ? throw new HttpRequestException($"{Name} is down")
            : Task.FromResult<decimal?>(price);
}
