using PriceAggregator.Domain;

namespace PriceAggregator.UnitTests.Domain;

public class InstrumentTests
{
    [Theory]
    [InlineData("BTC/USD")]
    [InlineData("btc/usd")]
    [InlineData("BTC-USD")]
    [InlineData("btc_usd")]
    [InlineData("  BTC/USD  ")]
    public void TryParse_SupportedFormats_NormalizesToCanonicalSymbol(string value)
    {
        Assert.True(Instrument.TryParse(value, out var instrument));
        Assert.Equal("BTC", instrument.BaseAsset);
        Assert.Equal("USD", instrument.QuoteAsset);
        Assert.Equal("BTC/USD", instrument.Symbol);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("BTCUSD")]
    [InlineData("B/USD")]
    [InlineData("BTC/US$")]
    [InlineData("BTC/USD/EUR")]
    public void TryParse_InvalidValues_Fails(string? value)
    {
        Assert.False(Instrument.TryParse(value, out _));
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.Equal(Instrument.Parse("btc-usd"), Instrument.Parse("BTC/USD"));
    }
}
