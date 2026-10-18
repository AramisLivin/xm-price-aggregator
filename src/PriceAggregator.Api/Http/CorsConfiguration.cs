namespace PriceAggregator.Api.Http;

internal static class CorsConfiguration
{
    public const string WebClientPolicy = "WebClient";
    private const string AllowedOriginsKey = "Cors:AllowedOrigins";

    public static IServiceCollection AddWebClientCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection(AllowedOriginsKey).Get<string[]>() ?? [];

        return services.AddCors(options => options.AddPolicy(WebClientPolicy, policy =>
            policy.WithOrigins(origins).WithMethods(HttpMethods.Get).AllowAnyHeader()));
    }
}
