using System.Net;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using ServiceWire.TcpIp;

namespace sadnerd.io.ATAS.BroadcastOrderEvents;

public class ServiceWireClientOrderEventHubDispatchService : IOrderEventHubDispatchService {
    private readonly IPEndPoint _ipEndpoint;

    public ServiceWireClientOrderEventHubDispatchService(
        IPEndPoint ipEndpoint
    )
    {
        _ipEndpoint = ipEndpoint;
    }


    public void NewOrder(NewOrderEventV1Message message)
    {
        using var client = new TcpClient<IOrderEventHubDispatchService>(_ipEndpoint);
        client.Proxy.NewOrder(message);
    }
}