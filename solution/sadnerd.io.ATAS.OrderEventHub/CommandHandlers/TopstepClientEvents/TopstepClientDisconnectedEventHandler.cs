using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers.TopstepClientEvents;

public class TopstepClientDisconnectedEventHandler : INotificationHandler<TopstepClientDisconnectedEvent>
{
    private readonly ProjectXTradeCopyManagerProvider _provider;
    private readonly TopstepConnectionManager _manager;

    public TopstepClientDisconnectedEventHandler(
        ProjectXTradeCopyManagerProvider provider,
        TopstepConnectionManager manager
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