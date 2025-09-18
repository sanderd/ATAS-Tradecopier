using Microsoft.AspNetCore.SignalR;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications.SignalR;

public class SignalRNotificationSink : INotificationSink
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationSink> _logger;

    public SignalRNotificationSink(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationSink> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task HandleNotificationAsync(Notification notification)
    {
        try
        {
            await _hubContext.Clients.Group("Notifications").SendAsync("NotificationReceived", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                Severity = notification.Severity.ToString(),
                notification.Timestamp,
                notification.Source,
                notification.Metadata
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification via SignalR");
        }
    }
}