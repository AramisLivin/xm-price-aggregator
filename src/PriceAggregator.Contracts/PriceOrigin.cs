using System.Text.Json.Serialization;

namespace PriceAggregator.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<PriceOrigin>))]
public enum PriceOrigin
{
    Datastore,
    ExternalSources,
}
