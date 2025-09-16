using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public class TopstepClientDisconnectedEvent(string connectionId) : INotification
{
    public string ConnectionId { get; } = connectionId;
}