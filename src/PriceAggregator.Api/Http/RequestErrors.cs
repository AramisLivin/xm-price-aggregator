using PriceAggregator.Application.Common;

namespace PriceAggregator.Api.Http;

internal static class RequestErrors
{
    public static AppError InvalidInstrument(string value) =>
        new(ErrorKind.Validation, "request.invalid_instrument",
            $"Invalid instrument '{value}', expected BASE-QUOTE (BTC-USD)");

    public static AppError MissingTimestamp(string parameter) =>
        new(ErrorKind.Validation, "request.missing_timestamp",
            $"Query parameter '{parameter}' is required");

    public static AppError InvalidTimestamp(string parameter, string value) =>
        new(ErrorKind.Validation, "request.invalid_timestamp",
            $"Query parameter '{parameter}' has invalid value '{value}', expected ISO 8601 (2023-01-01T00:00:00Z)");

    public static AppError TimestampNotWholeHour(string parameter, string value) =>
        new(ErrorKind.Validation, "request.timestamp_not_whole_hour",
            $"Query parameter '{parameter}' must be a whole hour, got '{value}'");
}
