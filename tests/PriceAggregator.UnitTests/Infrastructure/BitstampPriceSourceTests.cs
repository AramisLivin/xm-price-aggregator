using System.Net;
using PriceAggregator.Domain;
using PriceAggregator.Infrastructure.PriceSources.Bitstamp;
using PriceAggregator.UnitTests.Fakes;

namespace PriceAggregator.UnitTests.Infrastructure;

public class BitstampPriceSourceTests
{
    private const string BaseAddress = "https://www.bitstamp.net/";
    private static readonly Instrument BtcUsd = Instrument.Parse("BTC/USD");
    private static readonly HourlyTimestamp OneAm = HourlyTimestamp.FromUnixSeconds(1672534800);

    [Fact]
    public async Task GetClosePriceAsync_RequestsCandleThatEndsAtTimestamp()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, Response(1672531200, "16521.00"));
        using var client = handler.CreateClient(BaseAddress);

        await new BitstampPriceSource(client).GetClosePriceAsync(BtcUsd, OneAm, CancellationToken.None);

        Assert.Equal(
            "https://www.bitstamp.net/api/v2/ohlc/btcusd/?step=3600&limit=1&start=1672531200",
            handler.LastRequestUri?.ToString());
    }

    [Fact]
    public async Task GetClosePriceAsync_ParsesStringEncodedClosePrice()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, Response(1672531200, "16521.00"));
        using var client = handler.CreateClient(BaseAddress);

        var price = await new BitstampPriceSource(client).GetClosePriceAsync(BtcUsd, OneAm, CancellationToken.None);

        Assert.Equal(16521.00m, price);
    }

    [Fact]
    public async Task GetClosePriceAsync_CandleForDifferentHour_ReturnsNull()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, Response(1792314000, "81660.67"));
        using var client = handler.CreateClient(BaseAddress);

        var price = await new BitstampPriceSource(client).GetClosePriceAsync(BtcUsd, OneAm, CancellationToken.None);

        Assert.Null(price);
    }

    [Fact]
    public async Task GetClosePriceAsync_UnknownPair_ReturnsNull()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.NotFound, string.Empty);
        using var client = handler.CreateClient(BaseAddress);

        var price = await new BitstampPriceSource(client).GetClosePriceAsync(BtcUsd, OneAm, CancellationToken.None);

        Assert.Null(price);
    }

    [Fact]
    public async Task GetClosePriceAsync_ServerError_Throws()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
        using var client = handler.CreateClient(BaseAddress);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            new BitstampPriceSource(client).GetClosePriceAsync(BtcUsd, OneAm, CancellationToken.None));
    }

    private static string Response(long timestamp, string close) =>
        $$$"""
        {"data": {"pair": "BTC/USD", "ohlc": [{"timestamp": "{{{timestamp}}}", "open": "16530.00", "high": "16532.00", "low": "16507.00", "close": "{{{close}}}", "volume": "17.05"}]}}
        """;
}
