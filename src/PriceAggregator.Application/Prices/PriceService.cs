using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PriceAggregator.Application.Abstractions;
using PriceAggregator.Application.Common;
using PriceAggregator.Domain;
using PriceAggregator.Domain.Aggregation;

namespace PriceAggregator.Application.Prices;

public sealed partial class PriceService : IPriceService
{
    private readonly IReadOnlyList<IPriceSource> _sources;
    private readonly IPriceAggregationStrategy _aggregation;
    private readonly IPriceRepository _repository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<PriceService> _logger;
    private readonly TimeSpan _maxHistoryRange;

    public PriceService(
        IEnumerable<IPriceSource> sources,
        IPriceAggregationStrategy aggregation,
        IPriceRepository repository,
        IOptions<PriceOptions> options,
        TimeProvider timeProvider,
        ILogger<PriceService> logger)
    {
        _sources = sources.ToList();
        _aggregation = aggregation;
        _repository = repository;
        _timeProvider = timeProvider;
        _logger = logger;
        _maxHistoryRange = TimeSpan.FromDays(options.Value.MaxHistoryRangeDays);
        SupportedInstruments = options.Value.SupportedInstruments.Select(Instrument.Parse).ToList();
    }

    public IReadOnlyList<Instrument> SupportedInstruments { get; }

    public async Task<Result<PriceLookup>> GetPriceAsync(
        Instrument instrument,
        HourlyTimestamp timestamp,
        CancellationToken cancellationToken)
    {
        if (!SupportedInstruments.Contains(instrument))
        {
            return Result.Failure<PriceLookup>(PriceErrors.UnsupportedInstrument(instrument, SupportedSymbols()));
        }

        if (timestamp.Value > _timeProvider.GetUtcNow())
        {
            return Result.Failure<PriceLookup>(PriceErrors.TimestampInFuture(timestamp));
        }

        var stored = await _repository.FindAsync(instrument, timestamp, cancellationToken);
        if (stored is not null)
        {
            return Result.Success(new PriceLookup(stored, PriceOrigin.Datastore, []));
        }

        var outcomes = await Task.WhenAll(_sources.Select(s => FetchAsync(s, instrument, timestamp, cancellationToken)));
        var prices = outcomes.Where(o => o.Price is not null).Select(o => o.Price!).ToList();
        var unavailable = outcomes.Where(o => o.Failed).Select(o => o.Source).ToList();

        if (prices.Count == 0)
        {
            return Result.Failure<PriceLookup>(unavailable.Count > 0
                ? PriceErrors.SourcesUnavailable(instrument, timestamp)
                : PriceErrors.NoData(instrument, timestamp));
        }

        var aggregated = new AggregatedPrice(instrument, timestamp, _aggregation.Aggregate(prices), prices);

        if (unavailable.Count > 0)
        {
            LogPartialResultNotPersisted(instrument, timestamp, string.Join(", ", unavailable));
        }
        else if (!await _repository.TryAddAsync(aggregated, cancellationToken))
        {
            LogAlreadyPersisted(instrument, timestamp);
        }

        return Result.Success(new PriceLookup(aggregated, PriceOrigin.ExternalSources, unavailable));
    }

    public async Task<Result<IReadOnlyList<AggregatedPrice>>> GetHistoryAsync(
        Instrument instrument,
        HourlyTimestamp from,
        HourlyTimestamp to,
        CancellationToken cancellationToken)
    {
        if (!SupportedInstruments.Contains(instrument))
        {
            return Result.Failure<IReadOnlyList<AggregatedPrice>>(
                PriceErrors.UnsupportedInstrument(instrument, SupportedSymbols()));
        }

        if (from.Value > to.Value)
        {
            return Result.Failure<IReadOnlyList<AggregatedPrice>>(PriceErrors.InvalidRange(from, to));
        }

        if (to.Value - from.Value > _maxHistoryRange)
        {
            return Result.Failure<IReadOnlyList<AggregatedPrice>>(PriceErrors.RangeTooLarge(_maxHistoryRange.Days));
        }

        var prices = await _repository.GetRangeAsync(instrument, from, to, cancellationToken);
        return Result.Success(prices);
    }

    private async Task<SourceOutcome> FetchAsync(
        IPriceSource source,
        Instrument instrument,
        HourlyTimestamp timestamp,
        CancellationToken cancellationToken)
    {
        try
        {
            var price = await source.GetClosePriceAsync(instrument, timestamp, cancellationToken);
            if (price is null)
            {
                LogNoData(source.Name, instrument, timestamp);
                return new SourceOutcome(source.Name, null, Failed: false);
            }

            return new SourceOutcome(source.Name, new SourcePrice(source.Name, price.Value), Failed: false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            LogSourceFailed(ex, source.Name, instrument, timestamp);
            return new SourceOutcome(source.Name, null, Failed: true);
        }
    }

    private IEnumerable<string> SupportedSymbols() => SupportedInstruments.Select(i => i.Symbol);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Price source {Source} failed for {Instrument} at {Timestamp}")]
    private partial void LogSourceFailed(Exception exception, string source, Instrument instrument, HourlyTimestamp timestamp);

    [LoggerMessage(Level = LogLevel.Information, Message = "Price source {Source} has no data for {Instrument} at {Timestamp}")]
    private partial void LogNoData(string source, Instrument instrument, HourlyTimestamp timestamp);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Aggregated price for {Instrument} at {Timestamp} is partial (unavailable: {Sources}) and was not persisted")]
    private partial void LogPartialResultNotPersisted(Instrument instrument, HourlyTimestamp timestamp, string sources);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Aggregated price for {Instrument} at {Timestamp} was already persisted concurrently")]
    private partial void LogAlreadyPersisted(Instrument instrument, HourlyTimestamp timestamp);

    private sealed record SourceOutcome(string Source, SourcePrice? Price, bool Failed);
}
