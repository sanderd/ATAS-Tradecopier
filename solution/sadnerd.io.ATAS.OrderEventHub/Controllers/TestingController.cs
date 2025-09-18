using Microsoft.AspNetCore.Mvc;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.FeatureFlags;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;

namespace sadnerd.io.ATAS.OrderEventHub.Controllers;

[ApiController]
[Route("api/testing")]
public class TestingController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly INotificationService _notificationService;

    public TestingController(
        IFeatureFlagService featureFlagService,
        INotificationService notificationService)
    {
        _featureFlagService = featureFlagService;
        _notificationService = notificationService;
    }

    [HttpPost("notifications/send")]
    public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
    {
        if (!_featureFlagService.IsEnabled(FeatureFlags.NotificationTesting))
        {
            return NotFound();
        }

        var severity = Enum.TryParse<NotificationSeverity>(request.Severity, true, out var parsedSeverity) 
            ? parsedSeverity 
            : NotificationSeverity.Info;

        var notification = new Notification(
            $"Test {severity} Notification",
            $"This is a test notification with {severity.ToString().ToLower()} severity level. Generated at {DateTime.Now:HH:mm:ss}",
            severity,
            "Testing System",
            new Dictionary<string, object> { { "TestId", Guid.NewGuid().ToString() } }
        );

        await _notificationService.PublishNotificationAsync(notification);

        return Ok(new { message = "Test notification sent", notificationId = notification.Id });
    }

    [HttpPost("notifications/send-multiple")]
    public async Task<IActionResult> SendMultipleTestNotifications([FromBody] MultipleTestNotificationRequest request)
    {
        if (!_featureFlagService.IsEnabled(FeatureFlags.NotificationTesting))
        {
            return NotFound();
        }

        var severities = new[] { NotificationSeverity.Info, NotificationSeverity.Warning, NotificationSeverity.Error, NotificationSeverity.Critical };
        var sentNotifications = new List<string>();

        for (int i = 0; i < request.Count; i++)
        {
            var severity = severities[i % severities.Length];
            var notification = new Notification(
                $"Bulk Test {severity} #{i + 1}",
                $"This is bulk test notification #{i + 1} of {request.Count} with {severity.ToString().ToLower()} severity.",
                severity,
                "Bulk Testing System"
            );

            await _notificationService.PublishNotificationAsync(notification);
            sentNotifications.Add(notification.Id);

            // Small delay to avoid overwhelming
            if (i < request.Count - 1)
            {
                await Task.Delay(100);
            }
        }

        return Ok(new { message = $"Sent {request.Count} test notifications", notificationIds = sentNotifications });
    }
}

public class TestNotificationRequest
{
    public string Severity { get; set; } = "Info";
}

public class MultipleTestNotificationRequest
{
    public int Count { get; set; } = 5;
}