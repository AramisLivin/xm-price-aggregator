using System.Globalization;
using System.Text.Json;
using PriceAggregator.Application.Abstractions;
using PriceAggregator.Domain;

namespace PriceAggregator.Infrastructure.PriceSources.Bitfinex;

internal sealed class BitfinexPriceSource(HttpClient httpClient) : IPriceSource
{
    public const string SourceName = "Bitfinex";

    private const string HourlyCandlesPath = "v2/candles/trade:1h";
    private const int TimestampIndex = 0;
    private const int CloseIndex = 2;
    private const string ErrorMarker = "error";

    public string Name => SourceName;

    public async Task<decimal?> GetClosePriceAsync(
        Instrument instrument,
        HourlyTimestamp timestamp,
        CancellationToken cancellationToken)
    {
        var openTime = HourlyCandle.OpenTimeFor(timestamp).ToUnixTimeMilliseconds();
        var closeTime = openTime + (long)HourlyCandle.Duration.TotalMilliseconds - 1;
        var requestUri = string.Create(
            CultureInfo.InvariantCulture,
            $"{HourlyCandlesPath}:{ToSymbol(instrument)}/hist?start={openTime}&end={closeTime}&limit=1");

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return ParseClosePrice(document.RootElement, openTime);
    }

    private static string ToSymbol(Instrument instrument) =>
        instrument.BaseAsset.Length > 3 || instrument.QuoteAsset.Length > 3
            ? $"t{instrument.BaseAsset}:{instrument.QuoteAsset}"
            : $"t{instrument.BaseAsset}{instrument.QuoteAsset}";

    private static decimal? ParseClosePrice(JsonElement root, long openTime)
    {
        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"Unexpected Bitfinex response: expected an array, got {root.ValueKind}");
        }

        if (root.GetArrayLength() > 0 && root[0].ValueKind == JsonValueKind.String && root[0].GetString() == ErrorMarker)
        {
            throw new HttpRequestException($"Bitfinex returned an error: {root.GetRawText()}");
        }

        foreach (var candle in root.EnumerateArray())
        {
            if (candle.ValueKind == JsonValueKind.Array
                && candle.GetArrayLength() > CloseIndex
                && candle[TimestampIndex].GetInt64() == openTime)
            {
                return candle[CloseIndex].GetDecimal();
            }
        }

        return null;
    }
}
