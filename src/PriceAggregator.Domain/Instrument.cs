using System.Diagnostics.CodeAnalysis;

namespace PriceAggregator.Domain;

public sealed record Instrument
{
    private static readonly char[] Separators = ['/', '-', '_'];

    private Instrument(string baseAsset, string quoteAsset)
    {
        BaseAsset = baseAsset;
        QuoteAsset = quoteAsset;
    }

    public string BaseAsset { get; }

    public string QuoteAsset { get; }

    public string Symbol => $"{BaseAsset}/{QuoteAsset}";

    public static bool TryParse(string? value, [NotNullWhen(true)] out Instrument? instrument)
    {
        instrument = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Trim().ToUpperInvariant().Split(Separators);
        if (parts.Length != 2 || !IsValidAsset(parts[0]) || !IsValidAsset(parts[1]))
        {
            return false;
        }

        instrument = new Instrument(parts[0], parts[1]);
        return true;
    }

    public static Instrument Parse(string value) =>
        TryParse(value, out var instrument)
            ? instrument
            : throw new FormatException($"Invalid instrument '{value}', expected BASE/QUOTE (BTC/USD)");

    public override string ToString() => Symbol;

    private static bool IsValidAsset(string asset) => asset.Length >= 2 && asset.All(char.IsAsciiLetterOrDigit);
}
