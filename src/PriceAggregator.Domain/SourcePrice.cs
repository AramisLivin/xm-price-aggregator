namespace PriceAggregator.Domain;

public sealed record SourcePrice
{
    public SourcePrice(string source, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be positive");
        }

        Source = source;
        Price = price;
    }

    public string Source { get; }

    public decimal Price { get; }
}
