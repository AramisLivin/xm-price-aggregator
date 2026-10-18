using PriceAggregator.Domain;

namespace PriceAggregator.Application.Abstractions;

public interface IPriceSource
{
    string Name { get; }

    Task<double?> GetClosePriceAsync(Instrument instrument, HourlyTimestamp timestamp, CancellationToken cancellationToken);
}
