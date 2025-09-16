using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public record Contract
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("tickSize")]
    public decimal TickSize { get; init; }

    [JsonPropertyName("tickValue")]
    public decimal TickValue { get; init; }

    [JsonPropertyName("activeContract")]
    public bool ActiveContract { get; init; }
}

