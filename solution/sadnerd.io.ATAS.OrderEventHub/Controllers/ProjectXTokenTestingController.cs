using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.FeatureFlags;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.Controllers;

[ApiController]
[Route("api/testing")]
public class ProjectXTokenTestingController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IProjectXTokenCacheService _tokenCacheService;
    private readonly INotificationService _notificationService;

    public ProjectXTokenTestingController(
        IFeatureFlagService featureFlagService,
        IProjectXTokenCacheService tokenCacheService,
        INotificationService notificationService)
    {
        _featureFlagService = featureFlagService;
        _tokenCacheService = tokenCacheService;
        _notificationService = notificationService;
    }

    [HttpGet("token-cache/status")]
    public IActionResult GetTokenCacheStatus()
    {
        if (!_featureFlagService.IsEnabled(FeatureFlags.Testing))
        {
            return NotFound();
        }

        var status = new
        {
            CachedTokenCount = _tokenCacheService.GetCachedTokenCount(),
            CachedTokenKeys = _tokenCacheService.GetCachedTokenKeys().ToList()
        };

        return Ok(status);
    }

    [HttpDelete("token-cache/clear")]
    public async Task<IActionResult> ClearTokenCache([FromQuery] string? apiUrl = null, [FromQuery] string? apiUser = null)
    {
        if (!_featureFlagService.IsEnabled(FeatureFlags.Testing))
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(apiUrl) && !string.IsNullOrEmpty(apiUser))
        {
            // Clear specific token
            await _tokenCacheService.InvalidateTokenAsync(apiUrl, apiUser);
            
            await _notificationService.PublishNotificationAsync(new Notification(
                "Token Cache Cleared",
                $"Cleared cached token for {apiUrl}|{apiUser}",
                NotificationSeverity.Info,
                "Token Cache Testing"));
        }
        else
        {
            // Clear expired tokens
            _tokenCacheService.ClearExpiredTokens();
            
            await _notificationService.PublishNotificationAsync(new Notification(
                "Expired Tokens Cleared",
                "Cleared all expired tokens from cache",
                NotificationSeverity.Info,
                "Token Cache Testing"));
        }

        return Ok(new { message = "Token cache operation completed" });
    }

    [HttpPost("token-cache/test")]
    public async Task<IActionResult> TestTokenCaching([FromBody] TokenCacheTestRequest request)
    {
        if (!_featureFlagService.IsEnabled(FeatureFlags.Testing))
        {
            return NotFound();
        }

        try
        {
            // Test setting a token
            await _tokenCacheService.SetTokenAsync(
                request.ApiUrl, 
                request.ApiUser, 
                $"test-token-{Guid.NewGuid():N}", 
                TimeSpan.FromMinutes(request.ExpirationMinutes ?? 60));

            // Test retrieving the token
            var retrievedToken = await _tokenCacheService.GetTokenAsync(request.ApiUrl, request.ApiUser);

            await _notificationService.PublishNotificationAsync(new Notification(
                "Token Cache Test",
                $"Successfully tested token caching for {request.ApiUrl}|{request.ApiUser}. Token retrieved: {!string.IsNullOrEmpty(retrievedToken)}",
                NotificationSeverity.Info,
                "Token Cache Testing"));

            return Ok(new 
            { 
                message = "Token cache test completed",
                tokenRetrieved = !string.IsNullOrEmpty(retrievedToken),
                cachedTokenCount = _tokenCacheService.GetCachedTokenCount()
            });
        }
        catch (Exception ex)
        {
            await _notificationService.PublishNotificationAsync(new Notification(
                "Token Cache Test Failed",
                $"Failed to test token caching: {ex.Message}",
                NotificationSeverity.Error,
                "Token Cache Testing"));

            return BadRequest(new { error = ex.Message });
        }
    }
}

public class TokenCacheTestRequest
{
    public string ApiUrl { get; set; } = "https://api.test.com";
    public string ApiUser { get; set; } = "testuser";
    public int? ExpirationMinutes { get; set; } = 60;
}