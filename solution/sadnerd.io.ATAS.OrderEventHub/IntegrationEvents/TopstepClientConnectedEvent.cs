using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public class TopstepClientConnectedEvent(string accountName, string instrument, string connectionId) : INotification
{
    public string AccountName { get; } = accountName;
    public string Instrument { get; } = instrument;
    public string ConnectionId { get; } = connectionId;
}