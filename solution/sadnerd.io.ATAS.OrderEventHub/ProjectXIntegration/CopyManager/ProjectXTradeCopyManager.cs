using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

public class ProjectXTradeCopyManager : IDestinationManager
{
    private readonly IProjectXClient _projectXClient;
    private readonly ILogger<ProjectXTradeCopyManager> _logger;
    private readonly INotificationService _notificationService;
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

    private string ManagerInstanceId => $"{_projectXAccount}:{_projectXContract}";

    public ProjectXTradeCopyManager(
        IProjectXClient projectXClient,
        ILogger<ProjectXTradeCopyManager> logger,
        INotificationService notificationService,
        int contractMultiplier,
        string projectXAccount,
        string projectXContract
    )
    {
        _projectXClient = projectXClient;
        _logger = logger;
        _notificationService = notificationService;
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
                try
                {
                    var accounts = await _projectXClient.GetActiveAccounts(CancellationToken.None);
                    _projectXAccountId = accounts.FirstOrDefault(x => x.Name == _projectXAccount)?.Id;
                    
                    if (_projectXAccountId == null)
                    {
                        await PublishErrorNotificationAsync(
                            "Account Resolution Failed",
                            $"Could not find ProjectX account '{_projectXAccount}'. Please verify the account name and ensure it's active.",
                            NotificationSeverity.Critical,
                            new Dictionary<string, object> 
                            { 
                                { "AccountName", _projectXAccount },
                                { "AvailableAccounts", string.Join(", ", accounts.Select(a => a.Name)) }
                            });
                        
                        throw new InvalidOperationException($"ProjectX account '{_projectXAccount}' not found");
                    }

                    await PublishInfoNotificationAsync(
                        "Account Resolved",
                        $"Successfully connected to ProjectX account '{_projectXAccount}' (ID: {_projectXAccountId})",
                        new Dictionary<string, object> { { "AccountId", _projectXAccountId.Value } });
                }
                catch (Exception ex) when (!(ex is InvalidOperationException))
                {
                    await PublishErrorNotificationAsync(
                        "API Communication Error",
                        $"Failed to retrieve ProjectX accounts: {ex.Message}",
                        NotificationSeverity.Error,
                        new Dictionary<string, object> { { "Exception", ex.GetType().Name } });
                    throw;
                }
            }
        }
        finally
        {
            _accountDetailsSemaphore.Release();
        }

        return _projectXAccountId.Value;
    }

    public async Task<string> GetProjectXContract()
    {
        if (_projectXContractId != null) return _projectXContractId;

        await _accountDetailsSemaphore.WaitAsync();
        try
        {
            if (_projectXContractId == null)
            {
                try
                {
                    var contracts = await _projectXClient.GetContracts(_projectXContract, CancellationToken.None);
                    _projectXContractId = contracts.FirstOrDefault(x => x.Id.Replace(".", "").EndsWith(_projectXContract))?.Id;
                    
                    if (_projectXContractId == null)
                    {
                        await PublishErrorNotificationAsync(
                            "Contract Resolution Failed",
                            $"Could not find ProjectX contract '{_projectXContract}'. Please verify the contract symbol.",
                            NotificationSeverity.Critical,
                            new Dictionary<string, object> 
                            { 
                                { "ContractSymbol", _projectXContract },
                                { "AvailableContracts", string.Join(", ", contracts.Select(c => c.Id)) }
                            });
                        
                        throw new InvalidOperationException($"ProjectX contract '{_projectXContract}' not found");
                    }

                    await PublishInfoNotificationAsync(
                        "Contract Resolved",
                        $"Successfully resolved ProjectX contract '{_projectXContract}' to '{_projectXContractId}'",
                        new Dictionary<string, object> { { "ContractId", _projectXContractId } });
                }
                catch (Exception ex) when (!(ex is InvalidOperationException))
                {
                    await PublishErrorNotificationAsync(
                        "API Communication Error",
                        $"Failed to retrieve ProjectX contracts: {ex.Message}",
                        NotificationSeverity.Error,
                        new Dictionary<string, object> { { "Exception", ex.GetType().Name } });
                    throw;
                }
            }
        }
        finally
        {
            _accountDetailsSemaphore.Release();
        }

        return _projectXContractId;
    }
    
    public async Task MoveOrder()
    {
        await PublishErrorNotificationAsync(
            "Operation Not Supported",
            "Move order operation is not yet implemented",
            NotificationSeverity.Warning);
        
        throw new NotImplementedException();
    }

    public async Task CancelOrder(string atasOrderId)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        try
        {
            var orderMapItem = _orderMap.SingleOrDefault(map => map.AtasOrderId == atasOrderId);
            if (orderMapItem == default)
            {
                _state = ManagerState.Error;
                await PublishErrorNotificationAsync(
                    "Order Cancellation Failed",
                    $"Cannot cancel order '{atasOrderId}' - corresponding ProjectX order ID not found in order map",
                    NotificationSeverity.Critical,
                    new Dictionary<string, object> 
                    { 
                        { "AtasOrderId", atasOrderId },
                        { "MappedOrders", _orderMap.Count }
                    });
                
                _logger.LogCritical("Error cancelling the order - corresponding ProjectX order id not known");
                return;
            }

            var accountId = await GetProjectXAccountId();
            await _projectXClient.CancelOrder(accountId, int.Parse(orderMapItem.ProjectXOrderId));
            
            await PublishInfoNotificationAsync(
                "Order Cancelled",
                $"Successfully cancelled order '{atasOrderId}'",
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "ProjectXOrderId", orderMapItem.ProjectXOrderId }
                });
        }
        catch (Exception ex)
        {
            _state = ManagerState.Error;
            await PublishErrorNotificationAsync(
                "Order Cancellation Error",
                $"Failed to cancel order '{atasOrderId}': {ex.Message}",
                NotificationSeverity.Error,
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "Exception", ex.GetType().Name }
                });
            throw;
        }
    }

    public async Task CreateLimitOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        try
        {
            var accountId = await GetProjectXAccountId();
            var contract = await GetProjectXContract();
            var adjustedQuantity = (int)orderQuantity * _contractMultiplier;
            
            var result = await _projectXClient.CreateLimitOrder(
                accountId, 
                contract, 
                orderDirection == OrderDirection.Buy, 
                orderPrice, 
                adjustedQuantity);
            
            if (result == null)
            {
                await PublishErrorNotificationAsync(
                    "Limit Order Creation Failed",
                    $"ProjectX API returned null result for limit order creation",
                    NotificationSeverity.Error,
                    new Dictionary<string, object> 
                    { 
                        { "AtasOrderId", atasOrderId },
                        { "Direction", orderDirection.ToString() },
                        { "Price", orderPrice },
                        { "Quantity", orderQuantity }
                    });
                
                throw new InvalidOperationException("ProjectX API returned null result for limit order creation");
            }

            _orderMap.Add((atasOrderId, result.Value.ToString()));
            
            await PublishInfoNotificationAsync(
                "Limit Order Created",
                $"Successfully created {orderDirection.ToString().ToLower()} limit order for {orderQuantity} @ {orderPrice}",
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "ProjectXOrderId", result.Value.ToString() },
                    { "Direction", orderDirection.ToString() },
                    { "Price", orderPrice },
                    { "Quantity", orderQuantity },
                    { "AdjustedQuantity", adjustedQuantity }
                });
        }
        catch (Exception ex)
        {
            _state = ManagerState.Error;
            await PublishErrorNotificationAsync(
                "Limit Order Creation Error",
                $"Failed to create limit order: {ex.Message}",
                NotificationSeverity.Error,
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "Direction", orderDirection.ToString() },
                    { "Price", orderPrice },
                    { "Quantity", orderQuantity },
                    { "Exception", ex.GetType().Name }
                });
            throw;
        }
    }

    public async Task CreateMarketOrder(string atasOrderId, OrderDirection orderDirection, decimal orderQuantity)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        try
        {
            var accountId = await GetProjectXAccountId();
            var contract = await GetProjectXContract();
            var adjustedQuantity = (int)orderQuantity * _contractMultiplier;
            
            var result = await _projectXClient.CreateMarketOrder(
                accountId, 
                contract, 
                orderDirection == OrderDirection.Buy, 
                adjustedQuantity);
            
            await PublishInfoNotificationAsync(
                "Market Order Created",
                $"Successfully created {orderDirection.ToString().ToLower()} market order for {orderQuantity}",
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "Direction", orderDirection.ToString() },
                    { "Quantity", orderQuantity },
                    { "AdjustedQuantity", adjustedQuantity }
                });
        }
        catch (Exception ex)
        {
            _state = ManagerState.Error;
            await PublishErrorNotificationAsync(
                "Market Order Creation Error",
                $"Failed to create market order: {ex.Message}",
                NotificationSeverity.Error,
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "Direction", orderDirection.ToString() },
                    { "Quantity", orderQuantity },
                    { "Exception", ex.GetType().Name }
                });
            throw;
        }
    }

    public async Task CreateStopOrder(string atasOrderId, OrderDirection orderDirection, decimal orderPrice, decimal orderQuantity)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        try
        {
            var accountId = await GetProjectXAccountId();
            var contract = await GetProjectXContract();
            var adjustedQuantity = (int)orderQuantity * _contractMultiplier;
            
            var result = await _projectXClient.CreateStopOrder(
                accountId, 
                contract, 
                orderDirection == OrderDirection.Buy, 
                orderPrice, 
                adjustedQuantity);

            _orderMap.Add((atasOrderId, result.Value.ToString()));
            
            await PublishInfoNotificationAsync(
                "Stop Order Created",
                $"Successfully created {orderDirection.ToString().ToLower()} stop order for {orderQuantity} @ {orderPrice}",
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "ProjectXOrderId", result.Value.ToString() },
                    { "Direction", orderDirection.ToString() },
                    { "Price", orderPrice },
                    { "Quantity", orderQuantity },
                    { "AdjustedQuantity", adjustedQuantity }
                });
        }
        catch (Exception ex)
        {
            _state = ManagerState.Error;
            await PublishErrorNotificationAsync(
                "Stop Order Creation Error",
                $"Failed to create stop order: {ex.Message}",
                NotificationSeverity.Error,
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "Direction", orderDirection.ToString() },
                    { "Price", orderPrice },
                    { "Quantity", orderQuantity },
                    { "Exception", ex.GetType().Name }
                });
            throw;
        }
    }

    public async Task SetStopLoss(string atasOrderId, decimal orderPrice)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        try
        {
            var accountId = await GetProjectXAccountId();
            var contract = await GetProjectXContract();

            var openPositions = await _projectXClient.GetPositions(accountId);
            var openPosition = openPositions.FirstOrDefault(x => x.ContractId == contract);

            if (openPosition == null)
            {
                _state = ManagerState.Error;
                await PublishErrorNotificationAsync(
                    "Stop Loss Setup Failed",
                    $"Cannot set stop loss - no open position found for contract '{contract}'",
                    NotificationSeverity.Critical,
                    new Dictionary<string, object> 
                    { 
                        { "AtasOrderId", atasOrderId },
                        { "Contract", contract },
                        { "StopLossPrice", orderPrice },
                        { "OpenPositionsCount", openPositions.Count() }
                    });
                
                _logger.LogCritical("error setting stoploss because no topstep position exists");
                return;
            }

            _lastStoploss = orderPrice;
            await _projectXClient.SetStoploss(openPosition.Id, _lastStoploss, _lastTakeProfit, CancellationToken.None);
            
            await PublishInfoNotificationAsync(
                "Stop Loss Updated",
                $"Successfully set stop loss to {orderPrice}",
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "PositionId", openPosition.Id },
                    { "StopLossPrice", orderPrice },
                    { "TakeProfitPrice", _lastTakeProfit }
                });
        }
        catch (Exception ex)
        {
            _state = ManagerState.Error;
            await PublishErrorNotificationAsync(
                "Stop Loss Setup Error",
                $"Failed to set stop loss: {ex.Message}",
                NotificationSeverity.Error,
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "StopLossPrice", orderPrice },
                    { "Exception", ex.GetType().Name }
                });
            throw;
        }
    }

    public async Task SetTakeProfit(string atasOrderId, decimal orderPrice)
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        try
        {
            var accountId = await GetProjectXAccountId();
            var contract = await GetProjectXContract();
            
            var openPositions = await _projectXClient.GetPositions(accountId);
            var openPosition = openPositions.FirstOrDefault(x => x.ContractId == contract);

            if (openPosition == null)
            {
                _state = ManagerState.Error;
                await PublishErrorNotificationAsync(
                    "Take Profit Setup Failed",
                    $"Cannot set take profit - no open position found for contract '{contract}'",
                    NotificationSeverity.Critical,
                    new Dictionary<string, object> 
                    { 
                        { "AtasOrderId", atasOrderId },
                        { "Contract", contract },
                        { "TakeProfitPrice", orderPrice },
                        { "OpenPositionsCount", openPositions.Count() }
                    });
                
                _logger.LogCritical("error setting take profit because no topstep position exists");
                return;
            }
            
            _lastTakeProfit = orderPrice;
            await _projectXClient.SetStoploss(openPosition.Id, _lastStoploss, _lastTakeProfit, CancellationToken.None);
            
            await PublishInfoNotificationAsync(
                "Take Profit Updated",
                $"Successfully set take profit to {orderPrice}",
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "PositionId", openPosition.Id },
                    { "TakeProfitPrice", orderPrice },
                    { "StopLossPrice", _lastStoploss }
                });
        }
        catch (Exception ex)
        {
            _state = ManagerState.Error;
            await PublishErrorNotificationAsync(
                "Take Profit Setup Error",
                $"Failed to set take profit: {ex.Message}",
                NotificationSeverity.Error,
                new Dictionary<string, object> 
                { 
                    { "AtasOrderId", atasOrderId },
                    { "TakeProfitPrice", orderPrice },
                    { "Exception", ex.GetType().Name }
                });
            throw;
        }
    }

    public async Task FlattenPosition()
    {
        if (!IsConnected() || _state != ManagerState.Enabled) return;

        try
        {
            var accountId = await GetProjectXAccountId();
            var contract = await GetProjectXContract();
            
            // Close the position
            await _projectXClient.CloseContract(accountId, contract);

            // Cancel all open orders for this contract
            var openOrders = await _projectXClient.GetOpenOrders(accountId);
            var contractOrders = openOrders.Where(o => o.ContractId == contract).ToList();
            
            foreach (var order in contractOrders)
            {
                await _projectXClient.CancelOrder(accountId, order.Id);
            }

            // To be sure no orders triggered after closing it the first time
            await _projectXClient.CloseContract(accountId, contract);

            _lastStoploss = null;
            _lastTakeProfit = null;
            
            await PublishInfoNotificationAsync(
                "Position Flattened",
                $"Successfully flattened position and cancelled {contractOrders.Count} open orders",
                new Dictionary<string, object> 
                { 
                    { "Contract", contract },
                    { "CancelledOrders", contractOrders.Count }
                });
        }
        catch (Exception ex)
        {
            _state = ManagerState.Error;
            await PublishErrorNotificationAsync(
                "Position Flatten Error",
                $"Failed to flatten position: {ex.Message}",
                NotificationSeverity.Error,
                new Dictionary<string, object> 
                { 
                    { "Exception", ex.GetType().Name }
                });
            throw;
        }
    }

    public void SetState(ManagerState state)
    {
        var oldState = _state;
        _state = state;
        
        // Notify state changes
        if (oldState != state)
        {
            var severity = state switch
            {
                ManagerState.Enabled => NotificationSeverity.Info,
                ManagerState.Disabled => NotificationSeverity.Warning,
                ManagerState.Error => NotificationSeverity.Error,
                _ => NotificationSeverity.Info
            };

            _ = Task.Run(async () => await PublishInfoNotificationAsync(
                "Manager State Changed",
                $"Trade copy manager state changed from {oldState} to {state}",
                new Dictionary<string, object> 
                { 
                    { "OldState", oldState.ToString() },
                    { "NewState", state.ToString() }
                },
                severity));
        }
    }

    private async Task PublishInfoNotificationAsync(
        string title, 
        string message, 
        Dictionary<string, object>? metadata = null,
        NotificationSeverity severity = NotificationSeverity.Info)
    {
        var notification = new Notification(
            title,
            message,
            severity,
            ManagerInstanceId,
            metadata);

        await _notificationService.PublishNotificationAsync(notification);
    }

    private async Task PublishErrorNotificationAsync(
        string title, 
        string message, 
        NotificationSeverity severity,
        Dictionary<string, object>? metadata = null)
    {
        var notification = new Notification(
            title,
            message,
            severity,
            ManagerInstanceId,
            metadata);

        await _notificationService.PublishNotificationAsync(notification);
    }
}