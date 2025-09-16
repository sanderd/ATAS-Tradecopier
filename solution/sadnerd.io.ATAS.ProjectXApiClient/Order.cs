using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public record Order
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("accountId")]
    public int AccountId { get; init; }

    [JsonPropertyName("contractId")]
    public string ContractId { get; init; }

    [JsonPropertyName("creationTimestamp")]
    public DateTime CreationTimestamp { get; init; }

    [JsonPropertyName("updateTimestamp")]
    public DateTime UpdateTimestamp { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("type")]
    public int Type { get; init; }

    [JsonPropertyName("side")]
    public int Side { get; init; }

    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("limitPrice")]
    public decimal? LimitPrice { get; init; }

    [JsonPropertyName("stopPrice")]
    public decimal? StopPrice { get; init; }
}
