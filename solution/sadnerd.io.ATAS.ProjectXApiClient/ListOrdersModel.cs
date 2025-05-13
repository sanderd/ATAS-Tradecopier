using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public record ListOrdersModel
{
    [JsonPropertyName("orders")]
    public List<Order> Orders { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; init; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }
}
