// See https://aka.ms/new-console-template for more information

using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;
using ServiceWire.TcpIp;
using System.Net;

Console.WriteLine("Hello, World!");
Console.WriteLine("Press any key to send message");
Console.ReadKey();

var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);

using (var client = new TcpClient<IMyService>(ipEndpoint))
{
    var message = new NewOrderEventV1Message(
        "test",
        "test",
        123,
        123,
        "test",
        123
    );

    client.Proxy.NewOrder(message);
}