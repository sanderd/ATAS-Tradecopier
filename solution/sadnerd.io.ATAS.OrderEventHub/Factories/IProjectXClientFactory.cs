using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.Factories;

public interface IProjectXClientFactory
{
    IProjectXClient CreateClient(ProjectXVendor vendor);
    IProjectXClient CreateClient(ProjectXVendor vendor, int apiCredentialId);
}