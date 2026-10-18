using System.Globalization;

namespace PriceAggregator.Domain;

public readonly record struct HourlyTimestamp
{
    private HourlyTimestamp(DateTimeOffset utcValue) => Value = utcValue;

    public DateTimeOffset Value { get; }

    public static bool TryCreate(DateTimeOffset value, out HourlyTimestamp result)
    {
        var utc = value.ToUniversalTime();
        if (utc.Ticks % TimeSpan.TicksPerHour != 0)
        {
            result = default;
            return false;
        }

        result = new HourlyTimestamp(utc);
        return true;
    }

    public override string ToString() =>
        Value.ToString("yyyy-MM-dd HH:00 'UTC'", CultureInfo.InvariantCulture);
}
