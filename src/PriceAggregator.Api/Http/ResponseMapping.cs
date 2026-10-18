using PriceAggregator.Application.Prices;
using PriceAggregator.Contracts;
using PriceAggregator.Domain;
using ApplicationPriceOrigin = PriceAggregator.Application.Prices.PriceOrigin;
using ContractPriceOrigin = PriceAggregator.Contracts.PriceOrigin;

namespace PriceAggregator.Api.Http;

internal static class ResponseMapping
{
    public static AggregatedPriceResponse ToResponse(this PriceLookup lookup) =>
        new(
            lookup.Price.Instrument.Symbol,
            lookup.Price.Timestamp.Value,
            lookup.Price.Price,
            lookup.Origin switch
            {
                ApplicationPriceOrigin.Datastore => ContractPriceOrigin.Datastore,
                ApplicationPriceOrigin.ExternalSources => ContractPriceOrigin.ExternalSources,
                _ => throw new ArgumentOutOfRangeException(nameof(lookup), lookup.Origin, "Unknown price origin"),
            },
            lookup.Price.Sources.ToResponse(),
            lookup.UnavailableSources);

    public static PriceHistoryResponse ToHistoryResponse(
        this IReadOnlyList<AggregatedPrice> prices,
        Instrument instrument,
        HourlyTimestamp from,
        HourlyTimestamp to) =>
        new(
            instrument.Symbol,
            from.Value,
            to.Value,
            prices.Select(p => new HistoricalPriceResponse(p.Timestamp.Value, p.Price, p.Sources.ToResponse())).ToList());

    private static List<SourcePriceResponse> ToResponse(this IEnumerable<SourcePrice> sources) =>
        sources.Select(s => new SourcePriceResponse(s.Source, s.Price)).ToList();
}
