namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;

public enum NotificationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}

public class Notification
{
    public string Id { get; }
    public string Title { get; }
    public string Message { get; }
    public NotificationSeverity Severity { get; }
    public DateTime Timestamp { get; }
    public string? Source { get; }
    public Dictionary<string, object> Metadata { get; }

    public Notification(
        string title, 
        string message, 
        NotificationSeverity severity, 
        string? source = null,
        Dictionary<string, object>? metadata = null)
    {
        Id = Guid.NewGuid().ToString();
        Title = title;
        Message = message;
        Severity = severity;
        Timestamp = DateTime.UtcNow;
        Source = source;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
}