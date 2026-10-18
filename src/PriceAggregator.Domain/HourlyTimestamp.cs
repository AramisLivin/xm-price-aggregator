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

    public static HourlyTimestamp Create(DateTimeOffset value) =>
        TryCreate(value, out var result)
            ? result
            : throw new ArgumentException($"Timestamp {value:O} is not a whole hour", nameof(value));

    public static HourlyTimestamp FromUnixSeconds(long seconds) =>
        Create(DateTimeOffset.FromUnixTimeSeconds(seconds));

    public long ToUnixSeconds() => Value.ToUnixTimeSeconds();

    public override string ToString() =>
        Value.ToString("yyyy-MM-dd HH:00 'UTC'", CultureInfo.InvariantCulture);
}
