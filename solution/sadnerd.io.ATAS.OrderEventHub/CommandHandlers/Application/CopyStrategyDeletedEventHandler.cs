using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Admin;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers.Application;

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
            "CopyStrategy deleted: Id={StrategyId}, AtasAccountId={AtasAccountId}, ProjectXAccountId={ProjectXAccountId}, AtasContract={AtasContract}, ProjectXContract={ProjectXContract}, ContractMultiplier={ContractMultiplier}",
            notification.StrategyId,
            notification.AtasAccountId,
            notification.ProjectXAccountId,
            notification.AtasContract,
            notification.ProjectXContract,
            notification.ContractMultiplier
        );

        throw new NotImplementedException();

        // Additional logic (e.g., notifying other systems) can go here
        return Task.CompletedTask;
    }
}