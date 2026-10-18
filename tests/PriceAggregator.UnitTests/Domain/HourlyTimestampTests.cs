using PriceAggregator.Domain;

namespace PriceAggregator.UnitTests.Domain;

public class HourlyTimestampTests
{
    [Fact]
    public void TryCreate_WholeHourUtc_Succeeds()
    {
        var value = new DateTimeOffset(2023, 1, 1, 10, 0, 0, TimeSpan.Zero);

        var created = HourlyTimestamp.TryCreate(value, out var timestamp);

        Assert.True(created);
        Assert.Equal(value, timestamp.Value);
    }

    [Theory]
    [InlineData(0, 30, 0, 0)]
    [InlineData(0, 0, 15, 0)]
    [InlineData(0, 0, 0, 1)]
    public void TryCreate_NotWholeHour_Fails(int hours, int minutes, int seconds, int milliseconds)
    {
        var value = new DateTimeOffset(2023, 1, 1, 10 + hours, minutes, seconds, milliseconds, TimeSpan.Zero);

        Assert.False(HourlyTimestamp.TryCreate(value, out _));
    }

    [Fact]
    public void TryCreate_WithOffset_NormalizesToUtc()
    {
        var moscow = new DateTimeOffset(2023, 1, 1, 3, 0, 0, TimeSpan.FromHours(3));

        var timestamp = HourlyTimestamp.Create(moscow);

        Assert.Equal(TimeSpan.Zero, timestamp.Value.Offset);
        Assert.Equal(HourlyTimestamp.Create(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero)), timestamp);
    }

    [Fact]
    public void TryCreate_HalfHourOffsetZone_FailsBecauseUtcIsNotWholeHour()
    {
        var india = new DateTimeOffset(2023, 1, 1, 10, 0, 0, TimeSpan.FromMinutes(330));

        Assert.False(HourlyTimestamp.TryCreate(india, out _));
    }

    [Fact]
    public void Create_NotWholeHour_Throws()
    {
        var value = new DateTimeOffset(2023, 1, 1, 10, 30, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() => HourlyTimestamp.Create(value));
    }

    [Fact]
    public void FromUnixSeconds_RoundTrips()
    {
        var timestamp = HourlyTimestamp.FromUnixSeconds(1672531200);

        Assert.Equal(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), timestamp.Value);
        Assert.Equal(1672531200, timestamp.ToUnixSeconds());
    }

    [Fact]
    public void ToString_UsesInvariantUtcFormat()
    {
        var timestamp = HourlyTimestamp.FromUnixSeconds(1672531200);

        Assert.Equal("2023-01-01 00:00 UTC", timestamp.ToString());
    }
}
