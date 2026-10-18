# Price Aggregator

This is a small service that tells you what BTC/USD (or ETH/USD) cost at a given hour. It asks Bitstamp and
Bitfinex, takes the average of their close prices and saves it. If you ask for the same hour again, the answer
comes straight from the database. There is also a simple Blazor app so you can try it in the browser.

## How to run

All you need is the .NET 10 SDK. The SQLite database is created on the first start, so there is nothing else
to set up.

Open two terminals in the repository root and start the API and the frontend:

```bash
dotnet run --project src/PriceAggregator.Api --launch-profile http
dotnet run --project src/PriceAggregator.Web --launch-profile http
```

Then open http://localhost:5299 in your browser. The API itself runs on http://localhost:5229, and you can
play with it at http://localhost:5229/scalar.

To run the tests: `dotnet test`

### With Docker

Also you can run everything with one command instead:

```bash
docker compose up --build
```

The addresses are the same: http://localhost:5299 for the app and http://localhost:5229 for the API. The
database is kept in a Docker volume, so saved prices survive a restart. The Scalar page is only available
when running with `dotnet run`.

## Using the API

```bash
curl "http://localhost:5229/api/v1/prices/BTC-USD?timestamp=2023-01-01T01:00:00Z"
```

```json
{
  "instrument": "BTC/USD",
  "timestamp": "2023-01-01T01:00:00+00:00",
  "price": 16538.50,
  "origin": "ExternalSources",
  "sources": [
    { "source": "Bitstamp", "price": 16521.00 },
    { "source": "Bitfinex", "price": 16556 }
  ],
  "unavailableSources": []
}
```

Send the same request again and you'll see `"origin": "Datastore"`.

There are three endpoints:

- `GET /api/v1/prices/{instrument}?timestamp=...` gives the price at an hour
- `GET /api/v1/prices/{instrument}/history?from=...&to=...` gives the prices already saved for a time range
- `GET /api/v1/instruments` lists the supported instruments

When something goes wrong, you get a short message explaining what happened, plus a `code` field for
programs: 400 if the input is wrong, 404 if no exchange has data for that hour, 503 if the exchanges are down.

## The frontend

It's Blazor WebAssembly with Bootstrap. The app runs in the browser and talks to the API over plain HTTP and
JSON through one small class, `PriceApiClient`. The API address is in
`src/PriceAggregator.Web/wwwroot/appsettings.json`, and the API lets the frontend in through CORS. Both sides
use the same request and response types from `PriceAggregator.Contracts`, so they can't get out of sync.

There are two pages. On **Price lookup** you pick an instrument and an hour and see the average price, what
each exchange said and whether the answer came from the exchanges or from the database. On **History** you see
the prices saved for a time range. If the API returns an error, the page shows its message. Simple mistakes,
like an empty instrument or an hour in the future, are caught before anything is sent.

## Decisions I made

- **The price at 01:00 is the close of the 00:00–01:00 candle**, because the time point is the end of the
  hour. I also check that each exchange really returned that candle. Bitstamp quietly gives you the latest
  candle if you ask about the future, and I didn't want that to end up in the database.
- **Everything is in UTC.** If a timestamp has no offset, I treat it as UTC.
- **Only whole hours are allowed.** `10:30` is rejected, not rounded. Hours that haven't finished yet are
  rejected too, because their price can still change.
- **Prices are `decimal`.** It's a floating-point type that keeps prices like `1194.45` exact. SQLite stores
  it as text, which is fine because the database never does math on prices.
- **If one exchange is down**, you still get the price from the other one with a warning. That result isn't
  saved, so a later request can store the full average.
- **Supported instruments are set in the config** (`Prices:SupportedInstruments` in the API
  `appsettings.json`), so random input never reaches the exchanges.

## How the code is organized

```
src/
  PriceAggregator.Domain          instruments, hourly timestamps, the averaging formula
  PriceAggregator.Application     the price lookup logic
  PriceAggregator.Infrastructure  Bitstamp and Bitfinex clients, SQLite with EF Core
  PriceAggregator.Contracts       JSON types shared by the API and the frontend
  PriceAggregator.Api             REST endpoints
  PriceAggregator.Web             Blazor frontend
tests/
  PriceAggregator.UnitTests
  PriceAggregator.IntegrationTests
```

If you want to add another exchnage, just implement `IPriceSource`, register it in `Infrastructure/DependencyInjection.cs` next to
the other two and add its base address under `PriceSources` in the API `appsettings.json`.

Different formula - implement `IPriceAggregationStrategy` and swap it in `Application/DependencyInjection.cs`.
