using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.BrowserAutomation;

public class TopstepClientDisconnectedEvent(string connectionId) : INotification
{
    public string ConnectionId { get; } = connectionId;
}