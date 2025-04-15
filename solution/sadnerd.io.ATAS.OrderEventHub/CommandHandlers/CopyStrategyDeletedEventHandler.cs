using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers;

public class CopyStrategyDeletedEventHandler : INotificationHandler<CopyStrategyDeletedEvent>
{
    private readonly ILogger<CopyStrategyDeletedEventHandler> _logger;

    public CopyStrategyDeletedEventHandler(ILogger<CopyStrategyDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CopyStrategyDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "CopyStrategy deleted: Id={StrategyId}, AtasAccountId={AtasAccountId}, TopstepAccountId={TopstepAccountId}, AtasContract={AtasContract}, TopstepContract={TopstepContract}, ContractMultiplier={ContractMultiplier}",
            notification.StrategyId,
            notification.AtasAccountId,
            notification.TopstepAccountId,
            notification.AtasContract,
            notification.TopstepContract,
            notification.ContractMultiplier
        );

        throw new NotImplementedException();

        // Additional logic (e.g., notifying other systems) can go here
        return Task.CompletedTask;
    }
}