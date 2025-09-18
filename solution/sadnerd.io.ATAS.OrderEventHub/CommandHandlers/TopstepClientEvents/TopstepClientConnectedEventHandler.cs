using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.BrowserAutomation;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers.TopstepClientEvents;

public class TopstepClientConnectedEventHandler : INotificationHandler<TopstepClientConnectedEvent>
{
    private readonly ProjectXTradeCopyManagerProvider _provider;
    private readonly TopstepBrowserConnectionManager _manager;

    public TopstepClientConnectedEventHandler(
        ProjectXTradeCopyManagerProvider provider,
        TopstepBrowserConnectionManager manager
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