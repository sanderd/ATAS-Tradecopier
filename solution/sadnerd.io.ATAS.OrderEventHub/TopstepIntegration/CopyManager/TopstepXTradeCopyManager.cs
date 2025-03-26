using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

// Takes in events for a specific account & instrument combination.
public class TopstepXTradeCopyManager
{
    private readonly ITopstepBrowserAutomationClient _topstepBrowserAutomationClient;
    private string? _connectionId = null;
    private List<(string AtasOrderId, string TopstepOrderId)> _orderMap = new();
    

    public TopstepXTradeCopyManager(
        ITopstepBrowserAutomationClient topstepBrowserAutomationClient
    )
    {
        _topstepBrowserAutomationClient = topstepBrowserAutomationClient;
    }

    public void SetConnectionId(string connectionId)
    {
        _connectionId = connectionId;
    }

    public async Task MoveOrder()
    {

    }

    public async Task CancelOrder()
    {

    }

    public async Task CreateLimitOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (_connectionId == null) return;
        var result = await _topstepBrowserAutomationClient.CreateLimitOrder(_connectionId, orderDirection == OrderDirection.Buy ? true : false, orderPrice, (int)orderQuantity);
    }

    public async Task CreateMarketOrder(string atasOrderId, OrderDirection orderDirection, decimal orderQuantity)
    {

    }
}