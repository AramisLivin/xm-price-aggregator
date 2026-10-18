using PriceAggregator.Domain;

namespace PriceAggregator.Infrastructure.PriceSources;

internal static class HourlyCandle
{
    public static readonly TimeSpan Duration = TimeSpan.FromHours(1);

    public static DateTimeOffset OpenTimeFor(HourlyTimestamp closeTime) => closeTime.Value - Duration;
}
