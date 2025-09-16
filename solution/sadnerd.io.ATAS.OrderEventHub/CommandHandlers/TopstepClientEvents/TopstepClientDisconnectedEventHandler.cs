using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.BrowserAutomation;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers.TopstepClientEvents;

public class TopstepClientDisconnectedEventHandler : INotificationHandler<TopstepClientDisconnectedEvent>
{
    private readonly ProjectXTradeCopyManagerProvider _provider;
    private readonly TopstepBrowserConnectionManager _manager;

    public TopstepClientDisconnectedEventHandler(
        ProjectXTradeCopyManagerProvider provider,
        TopstepBrowserConnectionManager manager
    )
    {
        _provider = provider;
        _manager = manager;
    }

    public async Task Handle(TopstepClientDisconnectedEvent notification, CancellationToken cancellationToken)
    {
        _manager.Disconnect(notification.ConnectionId);
    }
}