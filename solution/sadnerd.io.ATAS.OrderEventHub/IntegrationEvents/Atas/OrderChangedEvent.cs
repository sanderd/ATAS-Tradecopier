using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Atas;

public class OrderChangedEvent : GenericOrderEvent<OrderChangedV1Message>, IIntegrationEvent
{
    public OrderChangedEvent(OrderChangedV1Message message) : base(message)
    {
    }
}