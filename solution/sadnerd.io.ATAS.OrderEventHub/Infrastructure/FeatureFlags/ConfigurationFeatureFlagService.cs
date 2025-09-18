using Microsoft.Extensions.Configuration;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.FeatureFlags;

public class ConfigurationFeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;
    private const string FeatureFlagPrefix = "FeatureFlags:";

    public ConfigurationFeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsEnabled(string featureName)
    {
        return _configuration.GetValue<bool>($"{FeatureFlagPrefix}{featureName}", false);
    }

    public T GetValue<T>(string featureName, T defaultValue = default)
    {
        return _configuration.GetValue<T>($"{FeatureFlagPrefix}{featureName}", defaultValue);
    }
}