FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY .editorconfig global.json Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet publish src/PriceAggregator.Web -c Release -o /app

FROM nginx:1.27-alpine
COPY src/PriceAggregator.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/wwwroot /usr/share/nginx/html
