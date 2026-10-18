using System.Net;
using System.Text.Json;
using PriceAggregator.Domain;
using PriceAggregator.Infrastructure.PriceSources.Bitfinex;
using PriceAggregator.UnitTests.Fakes;

namespace PriceAggregator.UnitTests.Infrastructure;

public class BitfinexPriceSourceTests
{
    private const string BaseAddress = "https://api-pub.bitfinex.com/";
    private const long MidnightMs = 1672531200000;
    private static readonly Instrument BtcUsd = Instrument.Parse("BTC/USD");
    private static readonly HourlyTimestamp OneAm = HourlyTimestamp.FromUnixSeconds(1672534800);

    [Fact]
    public async Task GetClosePriceAsync_RequestsOnlyTheCandleThatEndsAtTimestamp()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "[[1672531200000,16569,16556,16569,16541,8.8]]");
        using var client = handler.CreateClient(BaseAddress);

        var price = await new BitfinexPriceSource(client).GetClosePriceAsync(BtcUsd, OneAm, CancellationToken.None);

        Assert.Equal(16556m, price);
        Assert.Equal(
            "https://api-pub.bitfinex.com/v2/candles/trade:1h:tBTCUSD/hist?start=1672531200000&end=1672534799999&limit=1",
            handler.LastRequestUri?.ToString());
    }

    [Fact]
    public void ParseClosePrice_UsesCloseAtIndexTwoNotFour()
    {
        using var document = JsonDocument.Parse("[[1672531200000,1,2,3,4,5]]");

        Assert.Equal(2m, BitfinexPriceSource.ParseClosePrice(document.RootElement, MidnightMs));
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("[[1672534800000,1,2,3,4,5]]")]
    public void ParseClosePrice_NoMatchingCandle_ReturnsNull(string json)
    {
        using var document = JsonDocument.Parse(json);

        Assert.Null(BitfinexPriceSource.ParseClosePrice(document.RootElement, MidnightMs));
    }

    [Fact]
    public void ParseClosePrice_ErrorPayload_Throws()
    {
        using var document = JsonDocument.Parse("""["error",10020,"time_interval: invalid"]""");

        Assert.Throws<HttpRequestException>(() => BitfinexPriceSource.ParseClosePrice(document.RootElement, MidnightMs));
    }

    [Theory]
    [InlineData("BTC/USD", "tBTCUSD")]
    [InlineData("ETH/USD", "tETHUSD")]
    [InlineData("TESTBTC/TESTUSD", "tTESTBTC:TESTUSD")]
    public void ToSymbol_MapsInstrumentToBitfinexSymbol(string instrument, string expected)
    {
        Assert.Equal(expected, BitfinexPriceSource.ToSymbol(Instrument.Parse(instrument)));
    }
}
