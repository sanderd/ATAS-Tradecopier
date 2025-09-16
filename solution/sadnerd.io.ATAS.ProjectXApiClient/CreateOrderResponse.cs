using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public record CreateOrderResponse
{
    [JsonPropertyName("orderId")] 
    public int OrderId { get; init; }
}