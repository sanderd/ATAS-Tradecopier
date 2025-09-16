using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.SignalR.Models;

namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.SignalR;

public interface IProjectXBrowserAutomationClient
{
    Task<LimitOrderCreationResult> CreateLimitOrder(string connectionId, bool isLong, decimal orderPrice, int orderQuantity);
    Task<MarketOrderCreationResult> CreateMarketOrder(string connectionId, bool isLong, int orderQuantity);
    Task<CancelOrderResult> CancelOrder(string connectionId, string orderId);
    Task<SetTakeProfitResult> SetTakeProfit(string connectionId, decimal orderPrice);
    Task<SetStopLossResult> SetStopLoss(string connectionId, decimal orderPrice);
    Task<StopOrderCreationResult> CreateStopOrder(string connectionId, bool isLong, decimal orderPrice, int orderQuantity);
    Task<FlattenResult> Flatten(string connectionId);
}