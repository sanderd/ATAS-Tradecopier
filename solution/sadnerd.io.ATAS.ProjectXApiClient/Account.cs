using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public record Account
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("name")]
    public string Name { get; init; }
    [JsonPropertyName("balance")]
    public decimal Balance { get; init; }
    [JsonPropertyName("canTrade")]
    public bool CanTrade { get; init; }
    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; init; }
}