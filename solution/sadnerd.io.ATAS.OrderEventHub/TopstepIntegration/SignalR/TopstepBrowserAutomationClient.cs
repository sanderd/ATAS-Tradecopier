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

    public async Task<MarketOrderCreationResult> CreateMarketOrder(string connectionId, bool isLong, int orderQuantity)
    {
        var result = await _hubContext.Clients.Client(connectionId)?.InvokeAsync<MarketOrderCreationResult>("CreateMarketOrder", isLong, orderQuantity, CancellationToken.None);
        return result;
    }

    public async Task<CancelOrderResult> CancelOrder(string connectionId, string orderId)
    {
        var result = await _hubContext.Clients.Client(connectionId)?.InvokeAsync<CancelOrderResult>("CancelOrder", orderId, CancellationToken.None);
        return result;
    }

    public async Task<SetTakeProfitResult> SetTakeProfit(string connectionId, decimal orderPrice)
    {
        var result = await _hubContext.Clients.Client(connectionId)?.InvokeAsync<SetTakeProfitResult>("SetTakeProfit", orderPrice, CancellationToken.None);
        return result;
    }

    public async Task<SetStopLossResult> SetStopLoss(string connectionId, decimal orderPrice)
    {
        var result = await _hubContext.Clients.Client(connectionId)?.InvokeAsync<SetStopLossResult>("SetStopLoss", orderPrice, CancellationToken.None);
        return result;
    }

    public async Task<StopOrderCreationResult> CreateStopOrder(string connectionId, bool isLong, decimal orderPrice, int orderQuantity)
    {
        var result = await _hubContext.Clients.Client(connectionId)?.InvokeAsync<StopOrderCreationResult>("CreateStopOrder", isLong, orderPrice, orderQuantity, CancellationToken.None);
        return result;
    }
}