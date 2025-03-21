using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;

namespace sadnerd.io.ATAS.OrderEventHub;

public class OrderEventHubDispatchServiceImplementation : IOrderEventHubDispatchService
{
    public void NewOrder(NewOrderEventV1Message newOrder)
    {
        Console.WriteLine("NewOrder: " + newOrder.ToString());
    }

    public void NewOrder(bool test)
    {
        Console.WriteLine("neworder" + test.ToString());
    }
}