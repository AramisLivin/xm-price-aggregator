using Microsoft.AspNetCore.Http.HttpResults;
using PriceAggregator.Api.Http;
using PriceAggregator.Application.Prices;
using PriceAggregator.Contracts;
using PriceAggregator.Domain;

namespace PriceAggregator.Api.Endpoints;

internal static class PriceEndpoints
{
    public static IEndpointRouteBuilder MapPriceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Instruments, GetInstruments)
            .WithTags("Instruments")
            .WithName("GetInstruments")
            .WithSummary("Lists supported instruments.");

        var prices = app.MapGroup(ApiRoutes.Prices).WithTags("Prices");

        prices.MapGet("{instrument}", GetPriceAsync)
            .WithName("GetPrice")
            .WithSummary("Returns the aggregated close price at a whole-hour UTC time point.")
            .WithDescription("Served from the datastore when available; otherwise fetched from external sources, aggregated and persisted.");

        prices.MapGet("{instrument}/history", GetHistoryAsync)
            .WithName("GetPriceHistory")
            .WithSummary("Returns persisted aggregated prices within an inclusive UTC time range.");

        return app;
    }

    private static Ok<InstrumentsResponse> GetInstruments(IPriceService priceService) =>
        TypedResults.Ok(new InstrumentsResponse(priceService.SupportedInstruments.Select(i => i.Symbol).ToList()));

    private static async Task<Results<Ok<AggregatedPriceResponse>, ProblemHttpResult>> GetPriceAsync(
        string instrument,
        string? timestamp,
        IPriceService priceService,
        CancellationToken cancellationToken)
    {
        if (!Instrument.TryParse(instrument, out var parsedInstrument))
        {
            return RequestErrors.InvalidInstrument(instrument).ToProblem();
        }

        if (!TimestampQuery.TryParse(timestamp, nameof(timestamp), out var parsedTimestamp, out var error))
        {
            return error.ToProblem();
        }

        var result = await priceService.GetPriceAsync(parsedInstrument, parsedTimestamp, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value.ToResponse())
            : result.Error.ToProblem();
    }

    private static async Task<Results<Ok<PriceHistoryResponse>, ProblemHttpResult>> GetHistoryAsync(
        string instrument,
        string? from,
        string? to,
        IPriceService priceService,
        CancellationToken cancellationToken)
    {
        if (!Instrument.TryParse(instrument, out var parsedInstrument))
        {
            return RequestErrors.InvalidInstrument(instrument).ToProblem();
        }

        if (!TimestampQuery.TryParse(from, nameof(from), out var parsedFrom, out var fromError))
        {
            return fromError.ToProblem();
        }

        if (!TimestampQuery.TryParse(to, nameof(to), out var parsedTo, out var toError))
        {
            return toError.ToProblem();
        }

        var result = await priceService.GetHistoryAsync(parsedInstrument, parsedFrom, parsedTo, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value.ToHistoryResponse(parsedInstrument, parsedFrom, parsedTo))
            : result.Error.ToProblem();
    }
}
