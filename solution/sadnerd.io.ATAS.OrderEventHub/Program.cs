// See https://aka.ms/new-console-template for more information
using ServiceWire.TcpIp;
using System.Net;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts;
using sadnerd.io.ATAS.OrderEventHub;

var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);
var tcphost = new TcpHost(ipEndpoint);
tcphost.AddService<IMyService>(new MyServiceImplementation());
tcphost.Open();


Console.WriteLine("Hello, World!");
await Task.Delay(Timeout.Infinite);