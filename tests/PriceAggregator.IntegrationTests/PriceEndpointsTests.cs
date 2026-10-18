using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PriceAggregator.Contracts;

namespace PriceAggregator.IntegrationTests;

public sealed class PriceEndpointsTests : IDisposable
{
    private readonly PriceApiFactory _factory = new();
    private readonly HttpClient _client;

    public PriceEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetPrice_FirstRequestFetchesExternally_SecondIsServedFromDatastore()
    {
        var first = await _client.GetFromJsonAsync<AggregatedPriceResponse>(PriceUri("BTC-USD", "2023-01-01T00:00:00Z"));
        var second = await _client.GetFromJsonAsync<AggregatedPriceResponse>(PriceUri("BTC-USD", "2023-01-01T00:00:00Z"));

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(PriceOrigin.ExternalSources, first.Origin);
        Assert.Equal(PriceOrigin.Datastore, second.Origin);
        Assert.Equal(16530.25m, first.Price);
        Assert.Equal(first.Price, second.Price);
        Assert.Equal(
            [new SourcePriceResponse("Bitstamp", 16531.00m), new SourcePriceResponse("Bitfinex", 16529.50m)],
            second.Sources);
    }

    [Fact]
    public async Task GetPrice_TimestampWithoutOffset_IsTreatedAsUtc()
    {
        var response = await _client.GetFromJsonAsync<AggregatedPriceResponse>(PriceUri("btc-usd", "2023-01-01T00:00:00"));

        Assert.Equal(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), response?.Timestamp);
    }

    [Theory]
    [InlineData("BTC-USD", "2023-01-01T00:30:00Z", "request.timestamp_not_whole_hour")]
    [InlineData("BTC-USD", "not-a-date", "request.invalid_timestamp")]
    [InlineData("BTC-USD", "2999-01-01T00:00:00Z", "price.timestamp_in_future")]
    [InlineData("DOGE-USD", "2023-01-01T00:00:00Z", "price.unsupported_instrument")]
    [InlineData("BTCUSD", "2023-01-01T00:00:00Z", "request.invalid_instrument")]
    public async Task GetPrice_InvalidRequest_ReturnsProblemWithCode(string instrument, string timestamp, string expectedCode)
    {
        using var response = await _client.GetAsync(new Uri(PriceUri(instrument, timestamp), UriKind.Relative));
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetPrice_AllSourcesDown_Returns503()
    {
        _factory.Bitstamp.IsDown = true;
        _factory.Bitfinex.IsDown = true;

        using var response = await _client.GetAsync(new Uri(PriceUri("BTC-USD", "2023-01-01T00:00:00Z"), UriKind.Relative));
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("price.sources_unavailable", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetPrice_OneSourceDown_ReturnsPartialResultThatIsNotPersisted()
    {
        _factory.Bitfinex.IsDown = true;
        var partial = await _client.GetFromJsonAsync<AggregatedPriceResponse>(PriceUri("BTC-USD", "2023-01-01T00:00:00Z"));

        _factory.Bitfinex.IsDown = false;
        var complete = await _client.GetFromJsonAsync<AggregatedPriceResponse>(PriceUri("BTC-USD", "2023-01-01T00:00:00Z"));

        Assert.Equal(["Bitfinex"], partial?.UnavailableSources);
        Assert.Equal(16531.00m, partial?.Price);
        Assert.Equal(PriceOrigin.ExternalSources, complete?.Origin);
        Assert.Equal(2, complete?.Sources.Count);
    }

    [Fact]
    public async Task GetHistory_ReturnsOnlyPersistedPricesInRange()
    {
        foreach (var hour in new[] { "00", "01", "05" })
        {
            await _client.GetFromJsonAsync<AggregatedPriceResponse>(PriceUri("BTC-USD", $"2023-01-01T{hour}:00:00Z"));
        }

        var history = await _client.GetFromJsonAsync<PriceHistoryResponse>(
            $"{ApiRoutes.Prices}/BTC-USD/history?from=2023-01-01T00:00:00Z&to=2023-01-01T03:00:00Z");

        Assert.NotNull(history);
        Assert.Equal(
            [new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 1, 1, 1, 0, 0, TimeSpan.Zero)],
            history.Prices.Select(p => p.Timestamp));
    }

    [Fact]
    public async Task GetInstruments_ReturnsConfiguredInstruments()
    {
        var response = await _client.GetFromJsonAsync<InstrumentsResponse>(ApiRoutes.Instruments);

        Assert.Contains("BTC/USD", response?.Instruments ?? []);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static string PriceUri(string instrument, string timestamp) =>
        $"{ApiRoutes.Prices}/{instrument}?timestamp={Uri.EscapeDataString(timestamp)}";
}
