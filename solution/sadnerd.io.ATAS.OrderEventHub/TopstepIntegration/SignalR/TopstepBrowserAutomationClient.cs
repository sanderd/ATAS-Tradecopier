using Microsoft.AspNetCore.SignalR;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR.Models;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;

public class TopstepBrowserAutomationClient : ITopstepBrowserAutomationClient
{
    private readonly IHubContext<SignalRTopstepAutomationHub> _hubContext;

    public TopstepBrowserAutomationClient(IHubContext<SignalRTopstepAutomationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<LimitOrderCreationResult> CreateLimitOrder(string connectionId, bool isLong, decimal orderPrice, int orderQuantity)
    {
        var result = await _hubContext.Clients.Client(connectionId)?.InvokeAsync<LimitOrderCreationResult>("CreateLimitOrder", isLong, orderPrice, orderQuantity, CancellationToken.None);
        return result;
    }
}