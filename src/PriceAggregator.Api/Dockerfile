FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY .editorconfig global.json Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet publish src/PriceAggregator.Api -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir data && chown $APP_UID data
USER $APP_UID
ENV ConnectionStrings__Prices="Data Source=/app/data/prices.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "PriceAggregator.Api.dll"]
