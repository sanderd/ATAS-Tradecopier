using Microsoft.AspNetCore.SignalR;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications.SignalR;

public class NotificationHub : Hub
{
    public async Task JoinNotificationGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Notifications");
    }

    public async Task LeaveNotificationGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Notifications");
    }

    public override async Task OnConnectedAsync()
    {
        // Auto-join all clients to notification group
        await Groups.AddToGroupAsync(Context.ConnectionId, "Notifications");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Notifications");
        await base.OnDisconnectedAsync(exception);
    }
}