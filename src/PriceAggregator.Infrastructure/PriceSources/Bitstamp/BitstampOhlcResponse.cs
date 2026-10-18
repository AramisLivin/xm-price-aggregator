namespace PriceAggregator.Infrastructure.PriceSources.Bitstamp;

internal sealed record BitstampOhlcResponse(BitstampOhlcData? Data);

internal sealed record BitstampOhlcData(IReadOnlyList<BitstampCandle>? Ohlc);

internal sealed record BitstampCandle(long Timestamp, decimal Close);
