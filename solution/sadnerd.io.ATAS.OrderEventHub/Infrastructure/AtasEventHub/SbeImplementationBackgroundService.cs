using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.AtasEventHub;

public class SbeImplementationBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //    IPEndPoint ipEndPoint = new(IPAddress.Loopback, 5003);

        //    using Socket listener = new(
        //        ipEndPoint.AddressFamily,
        //        SocketType.Stream,
        //        ProtocolType.Tcp);



        //    try
        //    {
        //        listener.Bind(ipEndPoint);
        //        listener.Listen(100);

        //        var handler = await listener.AcceptAsync();
        //        Run(handler, stoppingToken);
        //    } catch(Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }


        //    stoppingToken.Register(() =>
        //    {
        //        listener?.Close(1);
        //    });
    }

    private async Task Run(Socket handler, CancellationToken stoppingToken)
    {
        while (true && !stoppingToken.IsCancellationRequested)
        {
            // Receive message.
            var buffer = new byte[1_024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);

            var eom = "<|EOM|>";
            if (response.IndexOf(eom) > -1 /* is end of message */)
            {
                Console.WriteLine(
                    $"Socket server received message: \"{response.Replace(eom, "")}\"");

                var ackMessage = "<|ACK|>";
                var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                await handler.SendAsync(echoBytes, 0);
                Console.WriteLine(
                    $"Socket server sent acknowledgment: \"{ackMessage}\"");

                break;
            }
            // Sample output:
            //    Socket server received message: "Hi friends 👋!"
            //    Socket server sent acknowledgment: "<|ACK|>"
        }
    }
}