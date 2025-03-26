using MediatR;
using Microsoft.Extensions.Logging;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers;

public class NewOrderCommandHandler : INotificationHandler<NewOrderEvent>
{
    private readonly TopstepXTradeCopyManagerProvider _tradeCopyManager;
    private readonly ILogger<NewOrderCommandHandler> _logger;

    public NewOrderCommandHandler(
        TopstepXTradeCopyManagerProvider tradeCopyManager,
        ILogger<NewOrderCommandHandler> logger
    )
    {
        _tradeCopyManager = tradeCopyManager;
        _logger = logger;
    }

    public async Task Handle(NewOrderEvent request, CancellationToken cancellationToken)
    {
        var instrument = request.Message.OrderSecurityId.Split('@')[0];
        var copyManagers = _tradeCopyManager.GetManagers(request.Message.OrderAccountId, instrument).ToList();

        if (!copyManagers.Any())
        {
            _logger.LogInformation("No copy managers configured for ATAS source {account} and instrument {instrument}", request.Message.OrderAccountId, request.Message.OrderSecurityId);
            return;
        }

        foreach(var copyManager in copyManagers)
        {
            switch (request.Message.OrderType)
            {
                case OrderType.Limit:
                    await copyManager.CreateLimitOrder(request.Message.OrderId, request.Message.OrderDirection, request.Message.OrderPrice, request.Message.OrderQuantityToFill);
                    break;
                case OrderType.Market:
                    await copyManager.CreateMarketOrder(request.Message.OrderId, request.Message.OrderDirection, request.Message.OrderQuantityToFill);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}