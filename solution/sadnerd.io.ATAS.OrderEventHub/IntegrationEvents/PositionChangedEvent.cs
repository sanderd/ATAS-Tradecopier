using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public class PositionChangedEvent : GenericOrderEvent<PositionChangedV1Message>, IIntegrationEvent
{
    public PositionChangedEvent(PositionChangedV1Message message) : base(message)
    {
    }
}