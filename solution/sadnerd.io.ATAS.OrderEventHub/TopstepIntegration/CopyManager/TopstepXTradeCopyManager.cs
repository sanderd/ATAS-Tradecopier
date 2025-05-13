using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

public class TopstepXTradeCopyManager : IDestinationManager
{
    private readonly ITopstepBrowserAutomationClient _topstepBrowserAutomationClient;
    private readonly IProjectXClient _projectXClient;
    private readonly ILogger<TopstepXTradeCopyManager> _logger;
    private readonly int _contractMultiplier;
    private readonly string _topstepAccount;
    private readonly string _topstepContract;
    private TopstepConnection? _connection = null;
    private List<(string AtasOrderId, string TopstepOrderId)> _orderMap = new();
    private ManagerState _state = ManagerState.Disabled;

    public int? _projectXAccountId = null;
    public string? _projectXContract = null;
    public SemaphoreSlim _accountDetailsSemaphore = new SemaphoreSlim(1);


    public TopstepXTradeCopyManager(
        ITopstepBrowserAutomationClient topstepBrowserAutomationClient,
        IProjectXClient projectXClient,
        ILogger<TopstepXTradeCopyManager> logger,
        int contractMultiplier,
        string topstepAccount,
        string topstepContract
    )
    {
        _topstepBrowserAutomationClient = topstepBrowserAutomationClient;
        _projectXClient = projectXClient;
        _logger = logger;
        _contractMultiplier = contractMultiplier;
        _topstepAccount = topstepAccount;
        _topstepContract = topstepContract;
    }

    public ManagerState State => _state;

    public void SetConnection(TopstepConnection connection)
    {
        _connection = connection;
    }

    public bool IsConnected()
    {
        return _connection?.Status == ConnectionStatus.Connected;
    }

    public async Task<int> GetProjectXAccountId()
    {
        if(_projectXAccountId != null) return _projectXAccountId.Value;
        
        await _accountDetailsSemaphore.WaitAsync();
        try
        {
            if (_projectXAccountId == null)
            {
                var accounts = await _projectXClient.GetActiveAccounts(CancellationToken.None);
                _projectXAccountId = accounts.FirstOrDefault(x => x.Name == _topstepAccount)?.Id;
                return _projectXAccountId ?? throw new NotImplementedException();
            }
        }
        finally
        {
            _accountDetailsSemaphore.Release();
        }

        return _projectXAccountId ?? throw new NotImplementedException();
    }

    public async Task<string> GetProjectXContract()
    {
        if (_projectXContract != null) return _projectXContract;

        await _accountDetailsSemaphore.WaitAsync();
        try
        {
            if (_projectXContract == null)
            {
                var contracts = await _projectXClient.GetContracts(_topstepContract, CancellationToken.None);
                _projectXContract = contracts.FirstOrDefault(x => x.Id.Replace(".", "").EndsWith(_topstepContract))?.Id;
                return _projectXContract ?? throw new NotImplementedException();
            }
        }
        finally
        {
            _accountDetailsSemaphore.Release();
        }

        return _projectXContract ?? throw new NotImplementedException();
    }
    

    public async Task MoveOrder()
    {
        throw new NotImplementedException();
    }

    public async Task CancelOrder(string atasOrderId)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        var orderMapItem = _orderMap.SingleOrDefault(map => map.AtasOrderId == atasOrderId);
        if (orderMapItem == default)
        {
            _state = ManagerState.Error;
            _logger.LogCritical("error canceling order because I don't know the topstepx order id");
            return;
        }

        var accountId = await GetProjectXAccountId();
        await _projectXClient.CancelOrder(accountId, int.Parse(orderMapItem.TopstepOrderId));
    }

    public async Task CreateLimitOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        var accountId = await GetProjectXAccountId();
        var contract = await GetProjectXContract();
        var result = await _projectXClient.CreateLimitOrder(accountId, contract, orderDirection == OrderDirection.Buy ? true : false, orderPrice, (int)orderQuantity * _contractMultiplier);
        
        if (result == null) throw new NotImplementedException();

        _orderMap.Add((atasOrderId, result.Value.ToString()));
    }

    public async Task CreateMarketOrder(string atasOrderId, OrderDirection orderDirection, decimal orderQuantity)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        var accountId = await GetProjectXAccountId();
        var contract = await GetProjectXContract();
        var result = await _projectXClient.CreateMarketOrder(accountId, contract, orderDirection == OrderDirection.Buy ? true : false, (int)orderQuantity * _contractMultiplier);
    }

    public async Task CreateStopOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        var accountId = await GetProjectXAccountId();
        var contract = await GetProjectXContract();
        var result = await _projectXClient.CreateStopOrder(accountId, contract, orderDirection == OrderDirection.Buy ? true : false, orderPrice, (int)orderQuantity * _contractMultiplier);

        _orderMap.Add((atasOrderId, result.Value.ToString()));
    }

    public async Task SetStopLoss(string atasOrderId, decimal orderPrice)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;
        var result = await _topstepBrowserAutomationClient.SetStopLoss(_connection.SignalRConnectionKey, orderPrice);

        if (!result.Success)
        {
            _state = ManagerState.Error;
            _logger.LogCritical("error setting stop loss {atasOrderId} {orderprice}}", atasOrderId, orderPrice);
            return;
        }
    }

    public async Task SetTakeProfit(string atasOrderId, decimal orderPrice)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;
        var result = await _topstepBrowserAutomationClient.SetTakeProfit(_connection.SignalRConnectionKey, orderPrice);

        if (!result.Success)
        {
            _state = ManagerState.Error;
            _logger.LogCritical("error setting take profit {atasOrderId} {orderprice}}", atasOrderId, orderPrice);
            return;
        }
    }

    public async Task FlattenPosition()
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        var accountId = await GetProjectXAccountId();
        var contract = await GetProjectXContract();
        await _projectXClient.CloseContract(accountId, contract);

        var openOrders = await _projectXClient.GetOpenOrders(accountId);
        foreach (var order in openOrders.Where(o => o.ContractId == contract))
        {
            await _projectXClient.CancelOrder(accountId, order.Id);
        }
    }

    public void SetState(ManagerState state)
    {
        _state = state;
    }
}

public enum ManagerState
{
    Disabled,
    Enabled,
    Error
}