using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

record TokenResponse
{
    [JsonPropertyName("token")]
    public string Token { get; init; }
    [JsonPropertyName("success")] 
    public bool Success { get; init; }
    [JsonPropertyName("errorCode")] 
    public int ErrorCode { get; init; }
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; init; }
}