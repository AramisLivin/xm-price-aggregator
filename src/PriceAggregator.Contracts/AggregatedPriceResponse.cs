namespace PriceAggregator.Contracts;

public sealed record AggregatedPriceResponse(
    string Instrument,
    DateTimeOffset Timestamp,
    decimal Price,
    PriceOrigin Origin,
    IReadOnlyList<SourcePriceResponse> Sources,
    IReadOnlyList<string> UnavailableSources);
