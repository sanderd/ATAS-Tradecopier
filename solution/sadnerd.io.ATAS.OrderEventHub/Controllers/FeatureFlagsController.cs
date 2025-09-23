using Microsoft.AspNetCore.Mvc;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.FeatureFlags;

namespace sadnerd.io.ATAS.OrderEventHub.Controllers;

[ApiController]
[Route("api/feature-flags")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;

    public FeatureFlagsController(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    [HttpGet("{flagName}")]
    public IActionResult GetFeatureFlag(string flagName)
    {
        var isEnabled = _featureFlagService.IsEnabled(flagName);
        return Ok(isEnabled);
    }

    [HttpGet]
    public IActionResult GetAllFeatureFlags()
    {
        var flags = new Dictionary<string, bool>
        {
            { FeatureFlags.Testing, _featureFlagService.IsEnabled(FeatureFlags.Testing) },
            { FeatureFlags.NotificationTesting, _featureFlagService.IsEnabled(FeatureFlags.NotificationTesting) }
        };

        return Ok(flags);
    }
}