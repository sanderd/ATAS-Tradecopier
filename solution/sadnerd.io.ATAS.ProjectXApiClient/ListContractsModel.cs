using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

record ListContractsModel
{
    [JsonPropertyName("contracts")]
    public List<Contract> Contracts { get; init; }
}