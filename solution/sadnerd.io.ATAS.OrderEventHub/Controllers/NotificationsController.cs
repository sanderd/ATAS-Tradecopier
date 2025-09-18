using Microsoft.AspNetCore.Mvc;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;

namespace sadnerd.io.ATAS.OrderEventHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public IActionResult GetNotifications(
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? since = null,
        [FromQuery] int? count = null)
    {
        NotificationSeverity? severityEnum = null;
        if (!string.IsNullOrEmpty(severity) && Enum.TryParse<NotificationSeverity>(severity, true, out var parsedSeverity))
        {
            severityEnum = parsedSeverity;
        }

        var notifications = count.HasValue 
            ? _notificationService.GetRecentNotifications(count.Value)
            : _notificationService.GetNotifications(severityEnum, since);

        return Ok(notifications);
    }

    [HttpDelete]
    public IActionResult ClearNotifications()
    {
        _notificationService.ClearNotifications();
        return Ok();
    }
}