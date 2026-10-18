namespace PriceAggregator.Contracts;

public sealed record PriceHistoryResponse(
    string Instrument,
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyList<HistoricalPriceResponse> Prices);

public sealed record HistoricalPriceResponse(DateTimeOffset Timestamp, decimal Price, IReadOnlyList<SourcePriceResponse> Sources);
