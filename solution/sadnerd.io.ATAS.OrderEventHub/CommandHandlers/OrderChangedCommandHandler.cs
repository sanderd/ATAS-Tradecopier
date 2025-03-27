using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers;

public class OrderChangedCommandHandler : INotificationHandler<OrderChangedEvent>
{
    private readonly TopstepXTradeCopyManagerProvider _tradeCopyManager;
    private readonly ILogger<OrderChangedCommandHandler> _logger;

    public OrderChangedCommandHandler(
        TopstepXTradeCopyManagerProvider tradeCopyManager,
        ILogger<OrderChangedCommandHandler> logger
    )
    {
        _tradeCopyManager = tradeCopyManager;
        _logger = logger;
    }

    public async Task Handle(OrderChangedEvent request, CancellationToken cancellationToken)
    {
        var instrument = request.Message.SecurityId.Split('@')[0];
        var copyManagers = _tradeCopyManager.GetManagers(request.Message.AccountId, instrument).ToList();

        if (!copyManagers.Any())
        {
            _logger.LogInformation("No copy managers configured for ATAS source {account} and instrument {instrument}", request.Message.AccountId, request.Message.SecurityId);
            return;
        }


        foreach (var copyManager in copyManagers)
        {
            switch (request.Message.Status)
            {
                case OrderStatus.Done when request.Message.Canceled && (!OrderHelper.IsTakeProfit(request.Message.Comment, request.Message.IsReduceOnly) && !OrderHelper.IsStopLoss(request.Message.Comment, request.Message.IsReduceOnly)):
                    await copyManager.CancelOrder(request.Message.OrderId);
                    break;
                case OrderStatus.Done when !request.Message.Canceled && OrderHelper.IsStopLoss(request.Message.Comment, request.Message.IsReduceOnly) && request.Message.UnfilledQuantity == 0:
                    await copyManager.FlattenPosition();
                    break;
                case OrderStatus.Done when !request.Message.Canceled && OrderHelper.IsTakeProfit(request.Message.Comment, request.Message.IsReduceOnly) && request.Message.UnfilledQuantity == 0:
                    await copyManager.FlattenPosition();
                    break;
                default:
                    _logger.LogInformation("Order changed event ignored");
                    break;
            }
        }
    }
}