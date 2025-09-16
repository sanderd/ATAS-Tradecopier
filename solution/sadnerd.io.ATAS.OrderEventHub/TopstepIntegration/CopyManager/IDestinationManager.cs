using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

public interface IDestinationManager
{
    ManagerState State { get; }
    bool IsConnected();
    Task MoveOrder();
    Task CancelOrder(string atasOrderId);
    Task CreateLimitOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity);
    Task CreateMarketOrder(string atasOrderId, OrderDirection orderDirection, decimal orderQuantity);
    Task SetTakeProfit(string atasOrderId, decimal orderPrice);
    Task CreateStopOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity);
    Task SetStopLoss(string atasOrderId, decimal orderPrice);
    Task FlattenPosition();

    void SetState(ManagerState state);
}