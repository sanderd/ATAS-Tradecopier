namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.FeatureFlags;

public interface IFeatureFlagService
{
    bool IsEnabled(string featureName);
    T GetValue<T>(string featureName, T defaultValue = default);
}