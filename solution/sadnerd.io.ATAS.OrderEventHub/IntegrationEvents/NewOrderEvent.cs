using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public class NewOrderEvent : GenericOrderEvent<NewOrderEventV1Message>, IIntegrationEvent
{
    public NewOrderEvent(NewOrderEventV1Message message) : base(message)
    {
    }
}