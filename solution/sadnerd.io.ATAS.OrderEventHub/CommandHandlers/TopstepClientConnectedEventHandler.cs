using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers;

public class TopstepClientConnectedEventHandler : INotificationHandler<TopstepClientConnectedEvent>
{
    private readonly TopstepXTradeCopyManagerProvider _provider;

    public TopstepClientConnectedEventHandler(
        TopstepXTradeCopyManagerProvider provider
    )
    {
        _provider = provider;
    }

    public async Task Handle(TopstepClientConnectedEvent notification, CancellationToken cancellationToken)
    {
        var managers = _provider.GetManagersByTopstepInformation(notification.AccountName, notification.Instrument);
        foreach (var manager in managers)
        {
            manager.SetConnectionId(notification.ConnectionId);
        }
    }
}