namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;

public interface INotificationSink
{
    Task HandleNotificationAsync(Notification notification);
}

public interface INotificationService
{
    Task PublishNotificationAsync(Notification notification);
    void AddSink(INotificationSink sink);
    void RemoveSink(INotificationSink sink);
    IEnumerable<Notification> GetRecentNotifications(int count = 100);
    IEnumerable<Notification> GetNotifications(NotificationSeverity? severity = null, DateTime? since = null);
    void ClearNotifications();
}