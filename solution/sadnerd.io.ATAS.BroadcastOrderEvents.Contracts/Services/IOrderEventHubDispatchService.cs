using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;

public interface IOrderEventHubDispatchService
{
    void NewOrder(NewOrderEventV1Message newOrder);
}