using Microsoft.Extensions.Logging;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications.Sinks;

public class ConsoleLoggingSink : INotificationSink
{
    private readonly ILogger<ConsoleLoggingSink> _logger;

    public ConsoleLoggingSink(ILogger<ConsoleLoggingSink> logger)
    {
        _logger = logger;
    }

    public Task HandleNotificationAsync(Notification notification)
    {
        var logLevel = GetLogLevel(notification.Severity);
        
        _logger.Log(logLevel, 
            "[{Source}] {Title}: {Message} | Metadata: {Metadata}", 
            notification.Source ?? "Unknown", 
            notification.Title, 
            notification.Message,
            string.Join(", ", notification.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}")));

        return Task.CompletedTask;
    }

    private static LogLevel GetLogLevel(NotificationSeverity severity)
    {
        return severity switch
        {
            NotificationSeverity.Info => LogLevel.Information,
            NotificationSeverity.Warning => LogLevel.Warning,
            NotificationSeverity.Error => LogLevel.Error,
            NotificationSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }
}