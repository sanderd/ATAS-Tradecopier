using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR.Models;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;

public interface ITopstepBrowserAutomationClient
{
    Task<LimitOrderCreationResult> CreateLimitOrder(string connectionId, bool isLong, decimal orderPrice, int orderQuantity);
}