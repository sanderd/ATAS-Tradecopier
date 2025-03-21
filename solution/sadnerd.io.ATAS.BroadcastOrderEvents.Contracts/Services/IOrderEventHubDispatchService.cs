using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;

public interface IOrderEventHubDispatchService
{
    void NewOrder(NewOrderEventV1Message message);
    void OrderChanged(OrderChangedV1Message message);
    void PositionChanged(PositionChangedV1Message message);
}