using PriceAggregator.Application.Abstractions;
using PriceAggregator.Domain;

namespace PriceAggregator.UnitTests.Fakes;

internal sealed class InMemoryPriceRepository : IPriceRepository
{
    private readonly Dictionary<(Instrument, HourlyTimestamp), AggregatedPrice> _prices = [];

    public IReadOnlyCollection<AggregatedPrice> Stored => _prices.Values;

    public Task<AggregatedPrice?> FindAsync(Instrument instrument, HourlyTimestamp timestamp, CancellationToken cancellationToken) =>
        Task.FromResult(_prices.GetValueOrDefault((instrument, timestamp)));

    public Task<IReadOnlyList<AggregatedPrice>> GetRangeAsync(
        Instrument instrument,
        HourlyTimestamp from,
        HourlyTimestamp to,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AggregatedPrice> result = _prices.Values
            .Where(p => p.Instrument == instrument && p.Timestamp.Value >= from.Value && p.Timestamp.Value <= to.Value)
            .OrderBy(p => p.Timestamp.Value)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<bool> TryAddAsync(AggregatedPrice price, CancellationToken cancellationToken) =>
        Task.FromResult(_prices.TryAdd((price.Instrument, price.Timestamp), price));
}
