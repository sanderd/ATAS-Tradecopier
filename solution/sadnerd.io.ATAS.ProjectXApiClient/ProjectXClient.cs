using Microsoft.Extensions.Options;
using RestSharp;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public class ProjectXClient : IProjectXClient, IDisposable
{
    private readonly IOptions<ProjectXClientOptions> _options;
    // https://restsharp.dev/docs/usage/example/
    
    private readonly RestClient _client;
    
    public ProjectXClient(
        HttpClient httpClient,
        IOptions<ProjectXClientOptions> options
    )
    {
        _options = options;
        
        _client = new RestClient(httpClient, new RestClientOptions
        {
            Authenticator = new ProjectXAuthenticator(_options.Value.ApiUrl, _options.Value.ApiKey, _options.Value.ApiUser),
        });
    }

    private RestRequest CreateRequest(string resource, Method method)
    {
        var request = new RestRequest(resource, method);
        request.AddHeader("Accept", "application/json");
        return request;
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<List<Account>> GetActiveAccounts(CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/account/search", Method.Post);
        request.AddJsonBody(new { onlyActiveAccounts = true });
        
        var response = await _client.PostAsync<ListAccountModel>(request, cancellationToken);

        return response!.Accounts;
    }

    public async Task<List<Contract>> GetContracts(string searchText, CancellationToken cancellationToken)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Contract/search", Method.Post);
        request.AddJsonBody(new {
            live = false,
            searchText = searchText
        });

        var response = await _client.PostAsync<ListContractsModel>(request, cancellationToken);

        return response!.Contracts;
    }

    public async Task<int?> CreateLimitOrder(int accountId, string contractId, bool isLong, decimal orderPrice, int orderQuantity, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Order/place", Method.Post);
        request.AddJsonBody(new
        {
            accountId,
            contractId,
            type = 1,
            side = isLong ? 0 : 1,
            size = orderQuantity,
            limitPrice = orderPrice,
            stopPrice = (decimal?)null,
            trailPrice = (decimal?)null,
            customTag = (string?)null,
            linkedOrderId = (string?)null
        });

        var response = await _client.PostAsync<CreateOrderResponse>(request, cancellationToken);
        return response?.OrderId;
    }

    public async Task<int?> CreateMarketOrder(int accountId, string contractId, bool isLong, int orderQuantity, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Order/place", Method.Post);
        request.AddJsonBody(new
        {
            accountId,
            contractId,
            type = 2,
            side = isLong ? 0 : 1,
            size = orderQuantity,
            stopPrice = (decimal?)null,
            trailPrice = (decimal?)null,
            customTag = (string?)null,
            linkedOrderId = (string?)null
        });

        var response = await _client.PostAsync<CreateOrderResponse>(request, cancellationToken);
        return response?.OrderId;
    }

    public async Task<int?> CreateStopOrder(int accountId, string contractId, bool isLong, decimal stopPrice, int orderQuantity, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Order/place", Method.Post);
        request.AddJsonBody(new
        {
            accountId = accountId,
            contractId = contractId,
            type = 4,
            side = isLong ? 0 : 1,
            size = orderQuantity,
            stopPrice = stopPrice,
            trailPrice = (decimal?)null,
            customTag = (string?)null,
            linkedOrderId = (string?)null
        });

        var response = await _client.PostAsync<CreateOrderResponse>(request, cancellationToken);
        return response?.OrderId;
    }

    public async Task CloseContract(int accountId, string contractId, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Position/closeContract", Method.Post);
        request.AddJsonBody(new
        {
            accountId = accountId,
            contractId = contractId
        });

        await _client.PostAsync(request, cancellationToken);
        //TODO: error handling
    }

    public async Task<List<Order>> GetOpenOrders(int accountId, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Order/searchOpen", Method.Post);
        request.AddJsonBody(new
        {
            accountId = accountId
        });

        var response = await _client.PostAsync<ListOrdersModel>(request, cancellationToken);

        return response!.Orders;
    }

    public async Task CancelOrder(int accountId, int orderId, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Order/cancel", Method.Post);
        request.AddJsonBody(new
        {
            accountId = accountId,
            orderId = orderId
        });

        await _client.PostAsync(request, cancellationToken);
        // TODO: error handling
    }

    public async Task SetStoploss(int positionId, decimal? stoploss, decimal? takeProfit, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.UserApiUrl}/Order/editStopLoss", Method.Post);
        request.AddJsonBody(new
        {
            positionId = positionId,
            stopLoss = stoploss,
            takeProfit = takeProfit
        });

        var result = await _client.PostAsync(request, cancellationToken);
    }

    public async Task<List<Position>> GetPositions(int accountId, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest($"{_options.Value.ApiUrl}/api/Position/searchOpen", Method.Post);
        request.AddJsonBody(new
        {
            accountId = accountId
        });

        var response = await _client.PostAsync<ListPositionsModel>(request, cancellationToken);

        return response!.Positions;
    }
}