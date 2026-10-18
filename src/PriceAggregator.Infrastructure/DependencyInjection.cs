using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PriceAggregator.Application.Abstractions;
using PriceAggregator.Infrastructure.Persistence;
using PriceAggregator.Infrastructure.PriceSources.Bitfinex;
using PriceAggregator.Infrastructure.PriceSources.Bitstamp;

namespace PriceAggregator.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "Prices";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PriceDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString(ConnectionStringName)));
        services.AddScoped<IPriceRepository, PriceRepository>();

        services.AddPriceSource<BitstampPriceSource>(configuration, BitstampPriceSource.SourceName);
        services.AddPriceSource<BitfinexPriceSource>(configuration, BitfinexPriceSource.SourceName);

        return services;
    }

    public static async Task MigratePriceDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PriceDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static void AddPriceSource<TSource>(this IServiceCollection services, IConfiguration configuration, string name)
        where TSource : class, IPriceSource
    {
        var baseAddressKey = $"PriceSources:{name}:BaseAddress";
        var baseAddress = configuration.GetValue<Uri>(baseAddressKey)
            ?? throw new InvalidOperationException($"{baseAddressKey} is not configured");

        services.AddHttpClient<TSource>(client => client.BaseAddress = baseAddress)
            .AddStandardResilienceHandler(resilience =>
            {
                resilience.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
                resilience.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
            });

        services.AddTransient<IPriceSource>(serviceProvider => serviceProvider.GetRequiredService<TSource>());
    }
}
