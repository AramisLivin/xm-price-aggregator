using PriceAggregator.Domain;

namespace PriceAggregator.Application.Prices;

public sealed record PriceLookup(AggregatedPrice Price, PriceOrigin Origin, IReadOnlyList<string> UnavailableSources);
