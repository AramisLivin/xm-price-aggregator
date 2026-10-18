using PriceAggregator.Domain;

namespace PriceAggregator.Application.Abstractions;

public interface IPriceSource
{
    string Name { get; }

    Task<decimal?> GetClosePriceAsync(Instrument instrument, HourlyTimestamp timestamp, CancellationToken cancellationToken);
}
