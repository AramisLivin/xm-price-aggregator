using System.ComponentModel.DataAnnotations;

namespace PriceAggregator.Application.Prices;

public sealed class PriceOptions
{
    public const string SectionName = "Prices";

    [MinLength(1)]
    public IReadOnlyList<string> SupportedInstruments { get; set; } = [];

    [Range(1, 3660)]
    public int MaxHistoryRangeDays { get; set; } = 366;
}
