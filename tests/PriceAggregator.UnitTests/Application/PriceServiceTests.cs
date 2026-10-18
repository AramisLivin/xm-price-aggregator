using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using PriceAggregator.Application.Common;
using PriceAggregator.Application.Prices;
using PriceAggregator.Domain;
using PriceAggregator.Domain.Aggregation;
using PriceAggregator.UnitTests.Fakes;

namespace PriceAggregator.UnitTests.Application;

public class PriceServiceTests
{
    private static readonly Instrument BtcUsd = Instrument.Parse("BTC/USD");
    private static readonly HourlyTimestamp Timestamp = HourlyTimestamp.FromUnixSeconds(1672534800);

    private readonly InMemoryPriceRepository _repository = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 10, 18, 12, 30, 0, TimeSpan.Zero));

    [Fact]
    public async Task GetPriceAsync_NotStored_FetchesAggregatesAndPersists()
    {
        var service = CreateService(FakePriceSource.Returning("A", 100m), FakePriceSource.Returning("B", 110m));

        var result = await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PriceOrigin.ExternalSources, result.Value.Origin);
        Assert.Equal(105m, result.Value.Price.Price);
        Assert.Equal(["A", "B"], result.Value.Price.Sources.Select(s => s.Source));
        Assert.Single(_repository.Stored);
    }

    [Fact]
    public async Task GetPriceAsync_AlreadyStored_ServesFromDatastoreWithoutCallingSources()
    {
        var source = FakePriceSource.Returning("A", 100m);
        var service = CreateService(source);
        await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        var result = await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PriceOrigin.Datastore, result.Value.Origin);
        Assert.Equal(1, source.Calls);
    }

    [Fact]
    public async Task GetPriceAsync_OneSourceFails_ReturnsPartialResultWithoutPersisting()
    {
        var service = CreateService(FakePriceSource.Returning("A", 100m), FakePriceSource.Failing("B"));

        var result = await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value.Price.Price);
        Assert.Equal(["B"], result.Value.UnavailableSources);
        Assert.Empty(_repository.Stored);
    }

    [Fact]
    public async Task GetPriceAsync_OneSourceHasNoData_AggregatesTheRestAndPersists()
    {
        var service = CreateService(FakePriceSource.Returning("A", 100m), FakePriceSource.Returning("B", null));

        var result = await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.UnavailableSources);
        Assert.Single(_repository.Stored);
    }

    [Fact]
    public async Task GetPriceAsync_SourceReturnsNegativePrice_TreatsSourceAsUnavailable()
    {
        var service = CreateService(FakePriceSource.Returning("A", 100m), FakePriceSource.Returning("B", -1m));

        var result = await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["B"], result.Value.UnavailableSources);
    }

    [Fact]
    public async Task GetPriceAsync_NoSourceHasData_ReturnsNotFound()
    {
        var service = CreateService(FakePriceSource.Returning("A", null), FakePriceSource.Returning("B", null));

        var result = await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.NotFound, result.Error.Kind);
    }

    [Fact]
    public async Task GetPriceAsync_AllSourcesFail_ReturnsUnavailable()
    {
        var service = CreateService(FakePriceSource.Failing("A"), FakePriceSource.Failing("B"));

        var result = await service.GetPriceAsync(BtcUsd, Timestamp, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorKind.Unavailable, result.Error.Kind);
    }

    [Fact]
    public async Task GetPriceAsync_FutureTimestamp_ReturnsValidationErrorWithoutCallingSources()
    {
        var source = FakePriceSource.Returning("A", 100m);
        var service = CreateService(source);
        var nextHour = HourlyTimestamp.Create(new DateTimeOffset(2026, 10, 18, 13, 0, 0, TimeSpan.Zero));

        var result = await service.GetPriceAsync(BtcUsd, nextHour, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("price.timestamp_in_future", result.Error.Code);
        Assert.Equal(0, source.Calls);
    }

    [Fact]
    public async Task GetPriceAsync_CurrentHourAlreadyClosed_IsAllowed()
    {
        var service = CreateService(FakePriceSource.Returning("A", 100m));
        var lastClosedHour = HourlyTimestamp.Create(new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero));

        var result = await service.GetPriceAsync(BtcUsd, lastClosedHour, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetPriceAsync_UnsupportedInstrument_ReturnsValidationError()
    {
        var service = CreateService(FakePriceSource.Returning("A", 100m));

        var result = await service.GetPriceAsync(Instrument.Parse("DOGE/USD"), Timestamp, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("price.unsupported_instrument", result.Error.Code);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsStoredPricesInRangeOrdered()
    {
        var service = CreateService(FakePriceSource.Returning("A", 100m));
        var first = HourlyTimestamp.FromUnixSeconds(1672531200);
        var second = HourlyTimestamp.FromUnixSeconds(1672534800);
        var outside = HourlyTimestamp.FromUnixSeconds(1672542000);
        foreach (var timestamp in new[] { outside, second, first })
        {
            await service.GetPriceAsync(BtcUsd, timestamp, CancellationToken.None);
        }

        var result = await service.GetHistoryAsync(BtcUsd, first, second, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([first, second], result.Value.Select(p => p.Timestamp));
    }

    [Fact]
    public async Task GetHistoryAsync_FromAfterTo_ReturnsValidationError()
    {
        var service = CreateService();

        var result = await service.GetHistoryAsync(
            BtcUsd, HourlyTimestamp.FromUnixSeconds(1672534800), HourlyTimestamp.FromUnixSeconds(1672531200), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("price.invalid_range", result.Error.Code);
    }

    [Fact]
    public async Task GetHistoryAsync_RangeTooLarge_ReturnsValidationError()
    {
        var service = CreateService();
        var from = HourlyTimestamp.FromUnixSeconds(1672531200);
        var to = HourlyTimestamp.Create(from.Value.AddDays(8));

        var result = await service.GetHistoryAsync(BtcUsd, from, to, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("price.range_too_large", result.Error.Code);
    }

    private PriceService CreateService(params FakePriceSource[] sources) =>
        new(
            sources,
            new AveragePriceAggregationStrategy(),
            _repository,
            Options.Create(new PriceOptions { SupportedInstruments = ["BTC/USD", "ETH/USD"], MaxHistoryRangeDays = 7 }),
            _timeProvider,
            NullLogger<PriceService>.Instance);
}
