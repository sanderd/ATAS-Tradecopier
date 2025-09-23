using System.Collections.Concurrent;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly ConcurrentBag<INotificationSink> _sinks = new();
    private readonly ConcurrentQueue<Notification> _notifications = new();
    private readonly ILogger<NotificationService> _logger;
    private const int MaxNotifications = 1000; // Keep last 1000 notifications in memory

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task PublishNotificationAsync(Notification notification)
    {
        // Store in memory
        _notifications.Enqueue(notification);
        
        // Trim old notifications if we exceed max
        while (_notifications.Count > MaxNotifications)
        {
            _notifications.TryDequeue(out _);
        }

        // Notify all sinks
        var tasks = _sinks.Select(sink => HandleSinkNotificationSafely(sink, notification));
        await Task.WhenAll(tasks);
    }

    private async Task HandleSinkNotificationSafely(INotificationSink sink, Notification notification)
    {
        try
        {
            await sink.HandleNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in notification sink {SinkType}", sink.GetType().Name);
        }
    }

    public void AddSink(INotificationSink sink)
    {
        _sinks.Add(sink);
    }

    public void RemoveSink(INotificationSink sink)
    {
        // ConcurrentBag doesn't support direct removal, would need a different collection if this becomes important
        _logger.LogWarning("RemoveSink not implemented for ConcurrentBag. Consider using a different collection if dynamic removal is needed.");
    }

    public IEnumerable<Notification> GetRecentNotifications(int count = 100)
    {
        return _notifications.ToArray()
            .OrderByDescending(n => n.Timestamp)
            .Take(count);
    }

    public IEnumerable<Notification> GetNotifications(NotificationSeverity? severity = null, DateTime? since = null)
    {
        var notifications = _notifications.ToArray().AsEnumerable();

        if (severity.HasValue)
        {
            notifications = notifications.Where(n => n.Severity == severity.Value);
        }

        if (since.HasValue)
        {
            notifications = notifications.Where(n => n.Timestamp >= since.Value);
        }

        return notifications.OrderByDescending(n => n.Timestamp);
    }

    public void ClearNotifications()
    {
        while (_notifications.TryDequeue(out _)) { }
    }
}