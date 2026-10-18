using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PriceAggregator.Application.Abstractions;
using PriceAggregator.Domain;

namespace PriceAggregator.Infrastructure.PriceSources.Bitstamp;

internal sealed class BitstampPriceSource(HttpClient httpClient) : IPriceSource
{
    public const string SourceName = "Bitstamp";

    private const string OhlcPath = "api/v2/ohlc";
    private const int HourStepSeconds = 3600;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string Name => SourceName;

    public async Task<decimal?> GetClosePriceAsync(
        Instrument instrument,
        HourlyTimestamp timestamp,
        CancellationToken cancellationToken)
    {
        var openTime = HourlyCandle.OpenTimeFor(timestamp).ToUnixTimeSeconds();
        var requestUri = string.Create(
            CultureInfo.InvariantCulture,
            $"{OhlcPath}/{ToPair(instrument)}/?step={HourStepSeconds}&limit=1&start={openTime}");

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<BitstampOhlcResponse>(SerializerOptions, cancellationToken);
        var candle = body?.Data?.Ohlc?.FirstOrDefault(c => c.Timestamp == openTime);
        return candle?.Close;
    }

    private static string ToPair(Instrument instrument) =>
        string.Concat(instrument.BaseAsset, instrument.QuoteAsset).ToLowerInvariant();
}
