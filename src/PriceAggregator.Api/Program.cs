using PriceAggregator.Api.Endpoints;
using PriceAggregator.Api.Http;
using PriceAggregator.Application;
using PriceAggregator.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddWebClientCors(builder.Configuration);
builder.Services.AddHealthChecks().AddInfrastructureHealthChecks();

var app = builder.Build();

await app.Services.MigratePriceDatabaseAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors(CorsConfiguration.WebClientPolicy);

app.MapPriceEndpoints();
app.MapHealthChecks("/health");

await app.RunAsync();
