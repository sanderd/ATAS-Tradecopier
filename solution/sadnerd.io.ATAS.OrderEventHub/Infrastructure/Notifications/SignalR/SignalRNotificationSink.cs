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
            // Explicitly create object with Pascal case property names to match JavaScript expectations
            var notificationData = new
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Severity = notification.Severity.ToString(),
                Timestamp = notification.Timestamp,
                Source = notification.Source,
                Metadata = notification.Metadata
            };

            await _hubContext.Clients.Group("Notifications").SendAsync("NotificationReceived", notificationData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification via SignalR");
        }
    }
}