using System.Net;
using Microsoft.Extensions.Options;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.OrderEventHub.Configuration;
using ServiceWire.TcpIp;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.AtasEventHub;

public class ServiceWireWorker : BackgroundService
{
    private readonly IOrderEventHubDispatchService _orderEventHubDispatchService;
    private readonly ServiceWireOptions _serviceWireOptions;
    private TcpHost _tcphost;

    public ServiceWireWorker(
        IOrderEventHubDispatchService orderEventHubDispatchService,
        IOptions<ServiceWireOptions> serviceWireOptions
    )
    {
        _orderEventHubDispatchService = orderEventHubDispatchService;
        _serviceWireOptions = serviceWireOptions.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ipEndpoint = new IPEndPoint(IPAddress.Parse(_serviceWireOptions.IpAddress), _serviceWireOptions.Port);
        _tcphost = new TcpHost(ipEndpoint);
        _tcphost.AddService(_orderEventHubDispatchService);
        _tcphost.Open();

        stoppingToken.Register(() =>
        {
            _tcphost.Close();
        });

        return Task.CompletedTask;
    }
}