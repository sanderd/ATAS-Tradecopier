using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;

namespace sadnerd.io.ATAS.OrderEventHub;

public class MyServiceImplementation : IMyService
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