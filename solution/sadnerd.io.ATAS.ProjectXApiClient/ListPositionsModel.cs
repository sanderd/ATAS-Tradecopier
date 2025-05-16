using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public record ListPositionsModel
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; init; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("positions")]
    public List<Position> Positions { get; init; }
}