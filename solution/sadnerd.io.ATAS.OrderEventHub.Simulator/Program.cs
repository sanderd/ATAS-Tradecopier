// See https://aka.ms/new-console-template for more information

using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;
using ServiceWire.TcpIp;
using System.Net;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;

Console.WriteLine("Hello, World!");
Console.WriteLine("Press any key to send message");
Console.ReadKey();

var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);

using (var client = new TcpClient<IOrderEventHubDispatchService>(ipEndpoint))
{
    var message = new NewOrderEventV1Message(
        OrderAccountId: "test",
        OrderId: "test",
        OrderType: OrderType.Limit,
        OrderPrice: 123,
        OrderQuantityToFill: 123,
        OrderSecurityId: "test",
        OrderDirection: OrderDirection.Buy,
        OrderTriggerPrice: 123
    );

    client.Proxy.NewOrder(message);
}