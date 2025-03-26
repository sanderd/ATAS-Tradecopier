using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR.Models;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;

public interface ITopstepBrowserAutomationClient
{
    Task<LimitOrderCreationResult> CreateLimitOrder(string connectionId, bool isLong, decimal orderPrice, int orderQuantity);
    Task<MarketOrderCreationResult> CreateMarketOrder(string connectionId, bool isLong, int orderQuantity);
    Task<CancelOrderResult> CancelOrder(string connectionId, string orderId);
    Task<SetTakeProfitResult> SetTakeProfit(string connectionId, decimal orderPrice);
    Task<SetStopLossResult> SetStopLoss(string connectionId, decimal orderPrice);
    Task<StopOrderCreationResult> CreateStopOrder(string connectionId, bool isLong, decimal orderPrice, int orderQuantity);
}