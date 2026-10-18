using System.Globalization;

namespace PriceAggregator.Web.Services;

internal static class DisplayFormat
{
    public static string Price(decimal price) => price.ToString("0.00", CultureInfo.InvariantCulture);

    public static string Timestamp(DateTimeOffset timestamp) =>
        timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);

    public static string QuoteCurrency(string instrument) => instrument[(instrument.IndexOf('/', StringComparison.Ordinal) + 1)..];

    public static DateTimeOffset LastClosedHour(DateTimeOffset now)
    {
        var utc = now.ToUniversalTime();
        return new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero);
    }
}
