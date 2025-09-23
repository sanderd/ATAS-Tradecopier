using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.FeatureFlags;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.Testing;

public class TokenCacheModel : PageModel
{
    private readonly IFeatureFlagService _featureFlagService;

    public TokenCacheModel(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public bool IsTestingEnabled { get; private set; }

    public void OnGet()
    {
        IsTestingEnabled = _featureFlagService.IsEnabled(FeatureFlags.NotificationTesting);
    }
}