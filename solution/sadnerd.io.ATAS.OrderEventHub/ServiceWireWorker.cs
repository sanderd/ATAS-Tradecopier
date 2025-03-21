using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using ServiceWire.TcpIp;

namespace sadnerd.io.ATAS.OrderEventHub;

public class ServiceWireWorker : BackgroundService
{
    private readonly IOrderEventHubDispatchService _orderEventHubDispatchService;
    private TcpHost _tcphost;

    public ServiceWireWorker(
        ILogger<ServiceWireWorker> logger,
        IOrderEventHubDispatchService orderEventHubDispatchService
    )
    {
        _orderEventHubDispatchService = orderEventHubDispatchService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);
        _tcphost = new TcpHost(ipEndpoint);
        _tcphost.AddService<IOrderEventHubDispatchService>(_orderEventHubDispatchService);
        _tcphost.Open();

        stoppingToken.Register(() =>
        {
            _tcphost.Close();
        });

        return Task.CompletedTask;
    }
}