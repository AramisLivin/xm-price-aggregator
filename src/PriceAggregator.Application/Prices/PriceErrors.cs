using PriceAggregator.Application.Common;
using PriceAggregator.Domain;

namespace PriceAggregator.Application.Prices;

public static class PriceErrors
{
    public static AppError UnsupportedInstrument(Instrument instrument, IEnumerable<string> supported) =>
        new(ErrorKind.Validation, "price.unsupported_instrument",
            $"Instrument {instrument} is not supported (supported: {string.Join(", ", supported)})");

    public static AppError TimestampInFuture(HourlyTimestamp timestamp) =>
        new(ErrorKind.Validation, "price.timestamp_in_future",
            $"Price for {timestamp} is not available yet");

    public static AppError InvalidRange(HourlyTimestamp from, HourlyTimestamp to) =>
        new(ErrorKind.Validation, "price.invalid_range",
            $"Range start {from} is after range end {to}");

    public static AppError RangeTooLarge(int maxDays) =>
        new(ErrorKind.Validation, "price.range_too_large",
            $"Range must not exceed {maxDays} days");

    public static AppError NoData(Instrument instrument, HourlyTimestamp timestamp) =>
        new(ErrorKind.NotFound, "price.no_data",
            $"No price data for {instrument} at {timestamp}");

    public static AppError SourcesUnavailable(Instrument instrument, HourlyTimestamp timestamp) =>
        new(ErrorKind.Unavailable, "price.sources_unavailable",
            $"Price sources are unavailable for {instrument} at {timestamp}, try again later");
}
