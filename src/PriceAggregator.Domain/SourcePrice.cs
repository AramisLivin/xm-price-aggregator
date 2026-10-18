namespace PriceAggregator.Domain;

public sealed record SourcePrice
{
    public SourcePrice(string source, double price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        if (!double.IsFinite(price) || price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be positive and finite");
        }

        Source = source;
        Price = price;
    }

    public string Source { get; }

    public double Price { get; }
}
