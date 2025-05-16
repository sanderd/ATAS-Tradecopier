using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public record Position
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("accountId")]
    public int AccountId { get; init; }

    [JsonPropertyName("contractId")]
    public string ContractId { get; init; }

    [JsonPropertyName("creationTimestamp")]
    public DateTime CreationTimestamp { get; init; }

    [JsonPropertyName("type")]
    public int Type { get; init; }

    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("averagePrice")]
    public decimal AveragePrice { get; init; }
}