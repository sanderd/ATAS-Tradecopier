using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Services;

public interface IProjectXVendorConfigurationService
{
    ProjectXVendorConfiguration GetVendorConfiguration(ProjectXVendor vendor);
    ProjectXVendorConfiguration GetVendorConfiguration(ProjectXVendor vendor, int apiCredentialId);
    IEnumerable<ProjectXVendorConfiguration> GetAllVendorConfigurations();
    Task<ProjectXApiCredential?> GetApiCredentialAsync(int id);
    Task<IEnumerable<ProjectXApiCredential>> GetApiCredentialsForVendorAsync(ProjectXVendor vendor);
}

public class ProjectXVendorConfigurationService : IProjectXVendorConfigurationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<ProjectXVendor, ProjectXVendorBaseConfiguration> _vendorBaseConfigurations;

    public ProjectXVendorConfigurationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Only store vendor-specific URLs and display names (not credentials)
        _vendorBaseConfigurations = new Dictionary<ProjectXVendor, ProjectXVendorBaseConfiguration>
        {
            {
                ProjectXVendor.TopstepX,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.TopstepX,
                    ApiUrl = "https://api.topstepx.com",
                    UserApiUrl = "https://userapi.topstepx.com",
                    DisplayName = "TopstepX"
                }
            },
            {
                ProjectXVendor.LucidTrading,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.LucidTrading,
                    ApiUrl = "https://api.lucidtrading.projectx.com",
                    UserApiUrl = "https://userapi.lucidtrading.projectx.com",
                    DisplayName = "Lucid Trading"
                }
            }
        };
    }

    public ProjectXVendorConfiguration GetVendorConfiguration(ProjectXVendor vendor)
    {
        // Get the first active API credential for this vendor
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderEventHubDbContext>();
        
        var apiCredential = context.ProjectXApiCredentials
            .Where(c => c.Vendor == vendor && c.IsActive)
            .OrderBy(c => c.Id)
            .FirstOrDefault();

        if (apiCredential == null)
        {
            throw new InvalidOperationException($"No active API credentials found for vendor {vendor}");
        }

        return CreateVendorConfiguration(vendor, apiCredential);
    }

    public ProjectXVendorConfiguration GetVendorConfiguration(ProjectXVendor vendor, int apiCredentialId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderEventHubDbContext>();
        
        var apiCredential = context.ProjectXApiCredentials
            .Where(c => c.Id == apiCredentialId && c.Vendor == vendor && c.IsActive)
            .FirstOrDefault();

        if (apiCredential == null)
        {
            throw new InvalidOperationException($"API credential with ID {apiCredentialId} not found or inactive for vendor {vendor}");
        }

        return CreateVendorConfiguration(vendor, apiCredential);
    }

    public IEnumerable<ProjectXVendorConfiguration> GetAllVendorConfigurations()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderEventHubDbContext>();
        
        var configs = new List<ProjectXVendorConfiguration>();
        
        foreach (var vendor in Enum.GetValues<ProjectXVendor>())
        {
            var apiCredential = context.ProjectXApiCredentials
                .Where(c => c.Vendor == vendor && c.IsActive)
                .OrderBy(c => c.Id)
                .FirstOrDefault();

            if (apiCredential != null)
            {
                configs.Add(CreateVendorConfiguration(vendor, apiCredential));
            }
        }

        return configs;
    }

    public async Task<ProjectXApiCredential?> GetApiCredentialAsync(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderEventHubDbContext>();
        
        return await context.ProjectXApiCredentials
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<ProjectXApiCredential>> GetApiCredentialsForVendorAsync(ProjectXVendor vendor)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderEventHubDbContext>();
        
        return await context.ProjectXApiCredentials
            .Where(c => c.Vendor == vendor)
            .OrderBy(c => c.DisplayName)
            .ToListAsync();
    }

    private ProjectXVendorConfiguration CreateVendorConfiguration(ProjectXVendor vendor, ProjectXApiCredential apiCredential)
    {
        if (!_vendorBaseConfigurations.TryGetValue(vendor, out var baseConfig))
        {
            throw new ArgumentException($"Vendor configuration not found for {vendor}");
        }

        return new ProjectXVendorConfiguration
        {
            Vendor = vendor,
            ApiKey = apiCredential.ApiKey,
            ApiUrl = baseConfig.ApiUrl,
            UserApiUrl = baseConfig.UserApiUrl,
            ApiUser = apiCredential.ApiUser,
            DisplayName = baseConfig.DisplayName
        };
    }

    private class ProjectXVendorBaseConfiguration
    {
        public ProjectXVendor Vendor { get; set; }
        public string ApiUrl { get; set; }
        public string UserApiUrl { get; set; }
        public string DisplayName { get; set; }
    }
}