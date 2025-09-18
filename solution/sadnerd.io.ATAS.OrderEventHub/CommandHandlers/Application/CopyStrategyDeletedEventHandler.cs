using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Admin;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers.Application;

public class CopyStrategyDeletedEventHandler : INotificationHandler<CopyStrategyDeletedEvent>
{
    private readonly ProjectXTradeCopyManagerProvider _managerProvider;
    private readonly ILogger<CopyStrategyDeletedEventHandler> _logger;

    public CopyStrategyDeletedEventHandler(
        ProjectXTradeCopyManagerProvider managerProvider,
        ILogger<CopyStrategyDeletedEventHandler> logger)
    {
        _managerProvider = managerProvider;
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

        try
        {
            _managerProvider.RemoveManager(
                notification.AtasAccountId,
                notification.AtasContract,
                notification.ProjectXAccountId,
                notification.ProjectXContract
            );

            _logger.LogInformation("Successfully removed manager for deleted strategy {StrategyId}", notification.StrategyId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove manager for deleted strategy {StrategyId}", notification.StrategyId);
        }

        return Task.CompletedTask;
    }
}