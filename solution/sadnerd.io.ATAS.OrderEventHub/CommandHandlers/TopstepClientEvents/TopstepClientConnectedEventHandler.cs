using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers.TopstepClientEvents;

public class TopstepClientConnectedEventHandler : INotificationHandler<TopstepClientConnectedEvent>
{
    private readonly ProjectXTradeCopyManagerProvider _provider;
    private readonly TopstepConnectionManager _manager;

    public TopstepClientConnectedEventHandler(
        ProjectXTradeCopyManagerProvider provider,
        TopstepConnectionManager manager
    )
    {
        _provider = provider;
        _manager = manager;
    }

    public async Task Handle(TopstepClientConnectedEvent notification, CancellationToken cancellationToken)
    {
        var connection = new TopstepConnection(ConnectionStatus.Connected, notification.ConnectionId, notification.AccountName, notification.Instrument);
        _manager.Add(connection);
        
        var managers = _provider.GetManagersByProjectXInformation(notification.AccountName, notification.Instrument);
        foreach (var manager in managers)
        {
            manager.SetState(ManagerState.Enabled);
        }
    }
}