using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PriceAggregator.Application.Prices;
using PriceAggregator.Domain;
using PriceAggregator.Domain.Aggregation;

namespace PriceAggregator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PriceOptions>()
            .Bind(configuration.GetSection(PriceOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                o => o.SupportedInstruments.All(i => Instrument.TryParse(i, out _)),
                "An instrument must be in BASE/QUOTE format (BTC/USD)")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IPriceAggregationStrategy, AveragePriceAggregationStrategy>();
        services.AddScoped<IPriceService, PriceService>();

        return services;
    }
}
