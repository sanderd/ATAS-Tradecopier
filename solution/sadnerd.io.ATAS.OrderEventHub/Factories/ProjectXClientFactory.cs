using Microsoft.Extensions.Options;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.Services;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.Factories;

public interface IProjectXClientFactory
{
    IProjectXClient CreateClient(ProjectXVendor vendor);
}

public class ProjectXClientFactory : IProjectXClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProjectXVendorConfigurationService _vendorConfigurationService;

    public ProjectXClientFactory(
        IHttpClientFactory httpClientFactory,
        IProjectXVendorConfigurationService vendorConfigurationService)
    {
        _httpClientFactory = httpClientFactory;
        _vendorConfigurationService = vendorConfigurationService;
    }

    public IProjectXClient CreateClient(ProjectXVendor vendor)
    {
        var vendorConfig = _vendorConfigurationService.GetVendorConfiguration(vendor);
        var httpClient = _httpClientFactory.CreateClient();
        
        var options = Options.Create(new ProjectXClientOptions
        {
            ApiKey = vendorConfig.ApiKey,
            ApiUrl = vendorConfig.ApiUrl,
            UserApiUrl = vendorConfig.UserApiUrl,
            ApiUser = vendorConfig.ApiUser
        });

        return new ProjectXClient(httpClient, options);
    }
}