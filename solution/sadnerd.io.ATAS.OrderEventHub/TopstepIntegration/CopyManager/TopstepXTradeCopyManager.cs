using Microsoft.Extensions.Logging;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

// Takes in events for a specific account & instrument combination.
public class TopstepXTradeCopyManager
{
    private readonly ITopstepBrowserAutomationClient _topstepBrowserAutomationClient;
    private readonly ILogger<TopstepXTradeCopyManager> _logger;
    private string? _connectionId = null;
    private List<(string AtasOrderId, string TopstepOrderId)> _orderMap = new();
    private bool _errorState = false;


    public TopstepXTradeCopyManager(
        ITopstepBrowserAutomationClient topstepBrowserAutomationClient,
        ILogger<TopstepXTradeCopyManager> logger
    )
    {
        _topstepBrowserAutomationClient = topstepBrowserAutomationClient;
        _logger = logger;
    }

    public void SetConnectionId(string connectionId)
    {
        _connectionId = connectionId;
    }

    public async Task MoveOrder()
    {

    }

    public async Task CancelOrder(string atasOrderId)
    {
        if (_connectionId == null || _errorState) return;

        var orderMapItem = _orderMap.SingleOrDefault(map => map.AtasOrderId == atasOrderId);
        if (orderMapItem == default)
        {
            _errorState = true;
            _logger.LogCritical("error canceling order because I don't know the topstepx order id");
            return;
        }

        var result = await _topstepBrowserAutomationClient.CancelOrder(_connectionId, orderMapItem.TopstepOrderId);
    }

    public async Task CreateLimitOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (_connectionId == null || _errorState) return;
        var result = await _topstepBrowserAutomationClient.CreateLimitOrder(_connectionId, orderDirection == OrderDirection.Buy ? true : false, orderPrice, (int)orderQuantity);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error creating limit order {atasOrderId} ({direction}), {orderprice}, {quantity}", atasOrderId, orderDirection, orderPrice, orderQuantity);
            return;
        }

        _orderMap.Add((atasOrderId, result.OrderId));
    }

    public async Task CreateMarketOrder(string atasOrderId, OrderDirection orderDirection, decimal orderQuantity)
    {
        if (_connectionId == null || _errorState) return;
        var result = await _topstepBrowserAutomationClient.CreateMarketOrder(_connectionId, orderDirection == OrderDirection.Buy ? true : false, (int)orderQuantity);
    }

    public async Task SetTakeProfit(string atasOrderId, decimal orderPrice)
    {
        if (_connectionId == null || _errorState) return;
        var result = await _topstepBrowserAutomationClient.SetTakeProfit(_connectionId, orderPrice);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error setting take profit {atasOrderId} {orderprice}}", atasOrderId, orderPrice);
            return;
        }
    }

    public async Task CreateStopOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (_connectionId == null || _errorState) return;
        var result = await _topstepBrowserAutomationClient.CreateStopOrder(_connectionId, orderDirection == OrderDirection.Buy ? true : false, orderPrice, (int)orderQuantity);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error creating stop order {atasOrderId} ({direction}), {orderprice}, {quantity}", atasOrderId, orderDirection, orderPrice, orderQuantity);
            return;
        }

        _orderMap.Add((atasOrderId, result.OrderId));
    }

    public async Task SetStopLoss(string atasOrderId, decimal orderPrice)
    {
        if (_connectionId == null || _errorState) return;
        var result = await _topstepBrowserAutomationClient.SetStopLoss(_connectionId, orderPrice);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error setting stop loss {atasOrderId} {orderprice}}", atasOrderId, orderPrice);
            return;
        }
    }
}