using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PriceAggregator.Application.Abstractions;
using PriceAggregator.Domain;

namespace PriceAggregator.Infrastructure.Persistence;

internal sealed class PriceRepository(PriceDbContext dbContext) : IPriceRepository
{
    private const int SqliteConstraintViolation = 19;

    public async Task<AggregatedPrice?> FindAsync(
        Instrument instrument,
        HourlyTimestamp timestamp,
        CancellationToken cancellationToken)
    {
        var symbol = instrument.Symbol;
        var unixTime = timestamp.ToUnixSeconds();

        var record = await dbContext.AggregatedPrices
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Instrument == symbol && p.Timestamp == unixTime, cancellationToken);

        return record?.ToDomain();
    }

    public async Task<IReadOnlyList<AggregatedPrice>> GetRangeAsync(
        Instrument instrument,
        HourlyTimestamp from,
        HourlyTimestamp to,
        CancellationToken cancellationToken)
    {
        var symbol = instrument.Symbol;
        var fromUnix = from.ToUnixSeconds();
        var toUnix = to.ToUnixSeconds();

        var records = await dbContext.AggregatedPrices
            .AsNoTracking()
            .Where(p => p.Instrument == symbol && p.Timestamp >= fromUnix && p.Timestamp <= toUnix)
            .OrderBy(p => p.Timestamp)
            .ToListAsync(cancellationToken);

        return records.Select(r => r.ToDomain()).ToList();
    }

    public async Task<bool> TryAddAsync(AggregatedPrice price, CancellationToken cancellationToken)
    {
        var record = price.ToRecord();
        dbContext.AggregatedPrices.Add(record);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraintViolation })
        {
            dbContext.Entry(record).State = EntityState.Detached;
            return false;
        }
    }
}
