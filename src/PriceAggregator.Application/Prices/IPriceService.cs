using PriceAggregator.Application.Common;
using PriceAggregator.Domain;

namespace PriceAggregator.Application.Prices;

public interface IPriceService
{
    IReadOnlyList<Instrument> SupportedInstruments { get; }

    Task<Result<PriceLookup>> GetPriceAsync(Instrument instrument, HourlyTimestamp timestamp, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<AggregatedPrice>>> GetHistoryAsync(
        Instrument instrument,
        HourlyTimestamp from,
        HourlyTimestamp to,
        CancellationToken cancellationToken);
}
