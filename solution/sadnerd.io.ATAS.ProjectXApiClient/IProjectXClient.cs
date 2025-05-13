namespace sadnerd.io.ATAS.ProjectXApiClient;

public interface IProjectXClient
{
    Task<List<Account>> GetActiveAccounts(CancellationToken cancellationToken);
    Task<List<Contract>> GetContracts(string searchText, CancellationToken cancellationToken);
    Task<int?> CreateLimitOrder(int accountId, string contractId, bool isLong, decimal orderPrice, int orderQuantity, CancellationToken cancellationToken = default);
    Task<int?> CreateMarketOrder(int accountId, string contractId, bool isLong, int contractMultiplier, CancellationToken cancellationToken = default);
    Task<int?> CreateStopOrder(int accountId, string contractId, bool isLong, decimal stopPrice, int orderQuantity, CancellationToken cancellationToken = default);
    Task CloseContract(int accountId, string contractId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetOpenOrders(int accountId, CancellationToken cancellationToken = default);
    Task CancelOrder(int accountId, int orderId, CancellationToken cancellationToken = default);
}