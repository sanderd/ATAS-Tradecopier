using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

public class ProjectXTradeCopyManager : IDestinationManager
{
    private readonly IProjectXClient _projectXClient;
    private readonly ILogger<ProjectXTradeCopyManager> _logger;
    private readonly int _contractMultiplier;
    private readonly string _projectXAccount;
    private readonly string _projectXContract;
    private List<(string AtasOrderId, string ProjectXOrderId)> _orderMap = new();
    private ManagerState _state = ManagerState.Disabled;

    private int? _projectXAccountId = null;
    private string? _projectXContractId = null;
    private SemaphoreSlim _accountDetailsSemaphore = new(1);
    private decimal? _lastStoploss = null;
    private decimal? _lastTakeProfit = null;


    public ProjectXTradeCopyManager(
        IProjectXClient projectXClient,
        ILogger<ProjectXTradeCopyManager> logger,
        int contractMultiplier,
        string projectXAccount,
        string projectXContract
    )
    {
        _projectXClient = projectXClient;
        _logger = logger;
        _contractMultiplier = contractMultiplier;
        _projectXAccount = projectXAccount;
        _projectXContract = projectXContract;
    }

    public ManagerState State => _state;

    public bool IsConnected()
    {
        return true; // Meh, browser connection no longer required
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
                _projectXAccountId = accounts.FirstOrDefault(x => x.Name == _projectXAccount)?.Id;
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
        if (_projectXContractId != null) return _projectXContractId;

        await _accountDetailsSemaphore.WaitAsync();
        try
        {
            if (_projectXContractId == null)
            {
                var contracts = await _projectXClient.GetContracts(_projectXContract, CancellationToken.None);
                _projectXContractId = contracts.FirstOrDefault(x => x.Id.Replace(".", "").EndsWith(_projectXContract))?.Id;
                return _projectXContractId ?? throw new NotImplementedException();
            }
        }
        finally
        {
            _accountDetailsSemaphore.Release();
        }

        return _projectXContractId ?? throw new NotImplementedException();
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
            _logger.LogCritical("Error cancelling the order - corresponding ProjectX order id not known");
            return;
        }

        var accountId = await GetProjectXAccountId();
        await _projectXClient.CancelOrder(accountId, int.Parse(orderMapItem.ProjectXOrderId));
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

        var accountId = await GetProjectXAccountId();
        var contract = await GetProjectXContract();

        var openPositions = await _projectXClient.GetPositions(accountId);
        var openPosition = openPositions.FirstOrDefault(x => x.ContractId == contract);

        if (openPosition == null)
        {
            _state = ManagerState.Error;
            _logger.LogCritical("error setting stoploss because no topstep position exists");
            return;
        }

        _lastStoploss = orderPrice;
        await _projectXClient.SetStoploss(openPosition.Id, _lastStoploss, _lastTakeProfit, CancellationToken.None);
    }

    public async Task SetTakeProfit(string atasOrderId, decimal orderPrice)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        var accountId = await GetProjectXAccountId();
        var contract = await GetProjectXContract();
        
        var openPositions = await _projectXClient.GetPositions(accountId);
        var openPosition = openPositions.FirstOrDefault(x => x.ContractId == contract);

        if (openPosition == null)
        {
            _state = ManagerState.Error;
            _logger.LogCritical("error setting stoploss because no topstep position exists");
            return;
        }
        
        _lastTakeProfit = orderPrice;
        await _projectXClient.SetStoploss(openPosition.Id, _lastStoploss, _lastTakeProfit, CancellationToken.None);
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

        // To be sure no orders triggered after closing it the first time
        await _projectXClient.CloseContract(accountId, contract);

        _lastStoploss = null;
        _lastTakeProfit = null;
    }

    public void SetState(ManagerState state)
    {
        _state = state;
    }
}