namespace PriceAggregator.Contracts;

public sealed record InstrumentsResponse(IReadOnlyList<string> Instruments);
