using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;

namespace sadnerd.io.ATAS.OrderEventHub;

public class OrderEventHubDispatchServiceImplementation : IOrderEventHubDispatchService
{
    public void NewOrder(NewOrderEventV1Message message)
    {
        Console.WriteLine("NewOrder: " + message.ToString());
    }

    public void OrderChanged(OrderChangedV1Message message)
    {
        Console.WriteLine("OrderChanged: " + message.ToString());
    }

    public void PositionChanged(PositionChangedV1Message message)
    {
        Console.WriteLine("PositionChanged: " + message.ToString());
    }
}