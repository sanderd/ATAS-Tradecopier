using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

record ListAccountModel
{
    [JsonPropertyName("accounts")]
    public List<Account> Accounts { get; init; }
}