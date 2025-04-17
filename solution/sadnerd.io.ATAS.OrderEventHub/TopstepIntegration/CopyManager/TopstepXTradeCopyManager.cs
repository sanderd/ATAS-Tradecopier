using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

public class TopstepXTradeCopyManager
{
    private readonly ITopstepBrowserAutomationClient _topstepBrowserAutomationClient;
    private readonly ILogger<TopstepXTradeCopyManager> _logger;
    private readonly int _contractMultiplier;
    private TopstepConnection? _connection = null;
    private List<(string AtasOrderId, string TopstepOrderId)> _orderMap = new();
    private bool _errorState = false;


    public TopstepXTradeCopyManager(
        ITopstepBrowserAutomationClient topstepBrowserAutomationClient,
        ILogger<TopstepXTradeCopyManager> logger,
        int contractMultiplier
    )
    {
        _topstepBrowserAutomationClient = topstepBrowserAutomationClient;
        _logger = logger;
        _contractMultiplier = contractMultiplier;
    }

    public bool ErrorState => _errorState;

    public void SetConnection(TopstepConnection connection)
    {
        _connection = connection;
    }

    public bool IsConnected()
    {
        return _connection?.Status == ConnectionStatus.Connected;
    }

    public async Task MoveOrder()
    {
        throw new NotImplementedException();
    }

    public async Task CancelOrder(string atasOrderId)
    {
        if (!IsConnected() || _errorState) return;

        var orderMapItem = _orderMap.SingleOrDefault(map => map.AtasOrderId == atasOrderId);
        if (orderMapItem == default)
        {
            _errorState = true;
            _logger.LogCritical("error canceling order because I don't know the topstepx order id");
            return;
        }

        var result = await _topstepBrowserAutomationClient.CancelOrder(_connection.SignalRConnectionKey, orderMapItem.TopstepOrderId);
    }

    public async Task CreateLimitOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (!IsConnected() || _errorState) return;
        var result = await _topstepBrowserAutomationClient.CreateLimitOrder(_connection.SignalRConnectionKey, orderDirection == OrderDirection.Buy ? true : false, orderPrice, (int)orderQuantity * _contractMultiplier);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error creating limit order {atasOrderId} ({direction}), {orderprice}, {quantity}", atasOrderId, orderDirection, orderPrice, orderQuantity * _contractMultiplier);
            return;
        }

        _orderMap.Add((atasOrderId, result.OrderId));
    }

    public async Task CreateMarketOrder(string atasOrderId, OrderDirection orderDirection, decimal orderQuantity)
    {
        if (!IsConnected() || _errorState) return;
        var result = await _topstepBrowserAutomationClient.CreateMarketOrder(_connection.SignalRConnectionKey, orderDirection == OrderDirection.Buy ? true : false, (int)orderQuantity * _contractMultiplier);
    }

    public async Task SetTakeProfit(string atasOrderId, decimal orderPrice)
    {
        if (!IsConnected() || _errorState) return;
        var result = await _topstepBrowserAutomationClient.SetTakeProfit(_connection.SignalRConnectionKey, orderPrice);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error setting take profit {atasOrderId} {orderprice}}", atasOrderId, orderPrice);
            return;
        }
    }

    public async Task CreateStopOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (!IsConnected() || _errorState) return;
        var result = await _topstepBrowserAutomationClient.CreateStopOrder(_connection.SignalRConnectionKey, orderDirection == OrderDirection.Buy ? true : false, orderPrice, (int)orderQuantity * _contractMultiplier);

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
        if (!IsConnected() || _errorState) return;
        var result = await _topstepBrowserAutomationClient.SetStopLoss(_connection.SignalRConnectionKey, orderPrice);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error setting stop loss {atasOrderId} {orderprice}}", atasOrderId, orderPrice);
            return;
        }
    }

    public async Task FlattenPosition()
    {
        if (!IsConnected() || _errorState) return;
        var result = await _topstepBrowserAutomationClient.Flatten(_connection.SignalRConnectionKey);

        if (!result.Success)
        {
            _errorState = true;
            _logger.LogCritical("error flattening position");
            return;
        }
    }

    public void ClearErrorState()
    {
        // TODO: Implement a way to wait for the origin to reach a 0 position
        _errorState = false;
    }
}