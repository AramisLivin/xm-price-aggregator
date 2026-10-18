using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PriceAggregator.Application.Abstractions;

namespace PriceAggregator.IntegrationTests;

internal sealed class PriceApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"prices-tests-{Guid.NewGuid():N}.db");

    public ConfigurablePriceSource Bitstamp { get; } = new("Bitstamp", 16531.00m);

    public ConfigurablePriceSource Bitfinex { get; } = new("Bitfinex", 16529.50m);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Prices", $"Data Source={_databasePath};Pooling=False");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPriceSource>();
            services.AddSingleton<IPriceSource>(Bitstamp);
            services.AddSingleton<IPriceSource>(Bitfinex);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        File.Delete(_databasePath);
    }
}
