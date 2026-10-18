using PriceAggregator.Domain;

namespace PriceAggregator.Application.Abstractions;

public interface IPriceRepository
{
    Task<AggregatedPrice?> FindAsync(Instrument instrument, HourlyTimestamp timestamp, CancellationToken cancellationToken);

    Task<IReadOnlyList<AggregatedPrice>> GetRangeAsync(
        Instrument instrument,
        HourlyTimestamp from,
        HourlyTimestamp to,
        CancellationToken cancellationToken);

    Task<bool> TryAddAsync(AggregatedPrice price, CancellationToken cancellationToken);
}
