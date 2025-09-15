using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Services;

public interface IProjectXVendorConfigurationService
{
    ProjectXVendorConfiguration GetVendorConfiguration(ProjectXVendor vendor);
    IEnumerable<ProjectXVendorConfiguration> GetAllVendorConfigurations();
}

public class ProjectXVendorConfigurationService : IProjectXVendorConfigurationService
{
    private readonly Dictionary<ProjectXVendor, ProjectXVendorConfiguration> _vendorConfigurations;

    public ProjectXVendorConfigurationService()
    {
        _vendorConfigurations = new Dictionary<ProjectXVendor, ProjectXVendorConfiguration>
        {
            {
                ProjectXVendor.TopstepX,
                new ProjectXVendorConfiguration
                {
                    Vendor = ProjectXVendor.TopstepX,
                    ApiKey = "6p9C6d/G5QMR7UZ/Bfsf2TjzKLLvJQtPqmTt/sVRqZM=",
                    ApiUrl = "https://api.topstepx.com",
                    UserApiUrl = "https://userapi.topstepx.com",
                    ApiUser = "sanderd",
                    DisplayName = "TopstepX"
                }
            },
            {
                ProjectXVendor.LucidTrading,
                new ProjectXVendorConfiguration
                {
                    Vendor = ProjectXVendor.LucidTrading,
                    ApiKey = "egz8Gp4gberhhDrRIZl/7R8JrxEyVrpGg62UySt9ybw=",
                    ApiUrl = "https://api.lucidtrading.projectx.com",
                    UserApiUrl = "http://userapi.lucidtrading.projectx.com",
                    ApiUser = "sanderd",
                    DisplayName = "Lucid Trading"
                }
            }
        };
    }

    public ProjectXVendorConfiguration GetVendorConfiguration(ProjectXVendor vendor)
    {
        if (_vendorConfigurations.TryGetValue(vendor, out var config))
        {
            return config;
        }

        throw new ArgumentException($"Vendor configuration not found for {vendor}");
    }

    public IEnumerable<ProjectXVendorConfiguration> GetAllVendorConfigurations()
    {
        return _vendorConfigurations.Values;
    }
}