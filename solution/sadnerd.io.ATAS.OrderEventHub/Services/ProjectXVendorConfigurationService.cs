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
                ProjectXVendor.Topstep,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.Topstep,
                    ApiUrl = "https://api.topstepx.com",
                    UserApiUrl = "https://userapi.topstepx.com",
                    DisplayName = "Topstep"
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
            },
            {
                ProjectXVendor.AlphaFutures,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.AlphaFutures,
                    ApiUrl = "https://api.alphaticks.projectx.com",
                    UserApiUrl = "https://userapi.alphaticks.projectx.com",
                    DisplayName = "Alpha Futures"
                }
            },
            {
                ProjectXVendor.TickTickTrader,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.TickTickTrader,
                    ApiUrl = "https://api.tickticktrader.projectx.com",
                    UserApiUrl = "https://userapi.tickticktrader.projectx.com",
                    DisplayName = "TickTickTrader"
                }
            },
            {
                ProjectXVendor.Bulenox,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.Bulenox,
                    ApiUrl = "https://api.bulenox.projectx.com",
                    UserApiUrl = "https://userapi.bulenox.projectx.com",
                    DisplayName = "Bulenox"
                }
            },
            {
                ProjectXVendor.TradeDay,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.TradeDay,
                    ApiUrl = "https://api.tradeday.projectx.com",
                    UserApiUrl = "https://userapi.tradeday.projectx.com",
                    DisplayName = "TradeDay"
                }
            },
            {
                ProjectXVendor.Blusky,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.Blusky,
                    ApiUrl = "https://api.blusky.projectx.com",
                    UserApiUrl = "https://userapi.blusky.projectx.com",
                    DisplayName = "Blusky"
                }
            },
            {
                ProjectXVendor.GoatFutures,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.GoatFutures,
                    ApiUrl = "https://api.goatfundedfutures.projectx.com",
                    UserApiUrl = "https://userapi.goatfundedfutures.projectx.com",
                    DisplayName = "Goat Futures"
                }
            },
            {
                ProjectXVendor.TheFuturesDesk,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.TheFuturesDesk,
                    ApiUrl = "https://api.thefuturesdesk.projectx.com",
                    UserApiUrl = "https://userapi.thefuturesdesk.projectx.com",
                    DisplayName = "The Futures Desk"
                }
            },
            {
                ProjectXVendor.DayTraders,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.DayTraders,
                    ApiUrl = "https://api.daytraders.projectx.com",
                    UserApiUrl = "https://userapi.daytraders.projectx.com",
                    DisplayName = "DayTraders"
                }
            },
            {
                ProjectXVendor.E8Futures,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.E8Futures,
                    ApiUrl = "https://api.e8.projectx.com",
                    UserApiUrl = "https://userapi.e8.projectx.com",
                    DisplayName = "E8 Futures"
                }
            },
            {
                ProjectXVendor.BlueGuardianFutures,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.BlueGuardianFutures,
                    ApiUrl = "https://api.blueguardianfutures.projectx.com",
                    UserApiUrl = "https://userapi.blueguardianfutures.projectx.com",
                    DisplayName = "Blue Guardian Futures"
                }
            },
            {
                ProjectXVendor.FuturesElite,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.FuturesElite,
                    ApiUrl = "https://api.futureselite.projectx.com",
                    UserApiUrl = "https://userapi.futureselite.projectx.com",
                    DisplayName = "FuturesElite"
                }
            },
            {
                ProjectXVendor.FXIFY,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.FXIFY,
                    ApiUrl = "https://api.fxifyfutures.projectx.com",
                    UserApiUrl = "https://userapi.fxifyfutures.projectx.com",
                    DisplayName = "FXIFY"
                }
            },
            {
                ProjectXVendor.HolaPrime,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.HolaPrime,
                    ApiUrl = "https://api.holaprime.projectx.com",
                    UserApiUrl = "https://userapi.holaprime.projectx.com",
                    DisplayName = "Hola Prime"
                }
            },
            {
                ProjectXVendor.TopOneFutures,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.TopOneFutures,
                    ApiUrl = "https://api.toponefutures.projectx.com",
                    UserApiUrl = "https://userapi.toponefutures.projectx.com",
                    DisplayName = "Top One Futures"
                }
            },
            {
                ProjectXVendor.FundingFutures,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.FundingFutures,
                    ApiUrl = "https://api.fundingfutures.projectx.com",
                    UserApiUrl = "https://userapi.fundingfutures.projectx.com",
                    DisplayName = "Funding Futures"
                }
            },
            {
                ProjectXVendor.TX3Funding,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.TX3Funding,
                    ApiUrl = "https://api.tx3funding.projectx.com",
                    UserApiUrl = "https://userapi.tx3funding.projectx.com",
                    DisplayName = "TX3 Funding"
                }
            },
            {
                ProjectXVendor.Tradeify,
                new ProjectXVendorBaseConfiguration
                {
                    Vendor = ProjectXVendor.Tradeify,
                    ApiUrl = "https://api.tradeify.projectx.com",
                    UserApiUrl = "https://userapi.tradeify.projectx.com",
                    DisplayName = "Tradeify"
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