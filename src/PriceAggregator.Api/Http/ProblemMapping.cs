using Microsoft.AspNetCore.Http.HttpResults;
using PriceAggregator.Application.Common;

namespace PriceAggregator.Api.Http;

internal static class ProblemMapping
{
    public static ProblemHttpResult ToProblem(this AppError error) =>
        TypedResults.Problem(
            statusCode: StatusCodeFor(error.Kind),
            title: TitleFor(error.Kind),
            detail: error.Message,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });

    private static int StatusCodeFor(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => StatusCodes.Status400BadRequest,
        ErrorKind.NotFound => StatusCodes.Status404NotFound,
        ErrorKind.Unavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string TitleFor(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => "Invalid request",
        ErrorKind.NotFound => "Price not found",
        ErrorKind.Unavailable => "Price sources unavailable",
        _ => "Unexpected error",
    };
}
