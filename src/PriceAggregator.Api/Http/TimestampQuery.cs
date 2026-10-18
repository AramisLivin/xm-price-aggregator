using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PriceAggregator.Application.Common;
using PriceAggregator.Domain;

namespace PriceAggregator.Api.Http;

internal static class TimestampQuery
{
    private const DateTimeStyles Styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    public static bool TryParse(
        string? value,
        string parameter,
        out HourlyTimestamp timestamp,
        [NotNullWhen(false)] out AppError? error)
    {
        timestamp = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = RequestErrors.MissingTimestamp(parameter);
            return false;
        }

        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, Styles, out var parsed))
        {
            error = RequestErrors.InvalidTimestamp(parameter, value);
            return false;
        }

        if (!HourlyTimestamp.TryCreate(parsed, out timestamp))
        {
            error = RequestErrors.TimestampNotWholeHour(parameter, value);
            return false;
        }

        error = null;
        return true;
    }
}
