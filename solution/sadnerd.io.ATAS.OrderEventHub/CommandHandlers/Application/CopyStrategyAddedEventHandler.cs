using MediatR;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Admin;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers.Application;

public class CopyStrategyAddedEventHandler : INotificationHandler<CopyStrategyAddedEvent>
{
    private readonly ProjectXTradeCopyManagerProvider _provider;
    private readonly OrderEventHubDbContext _dbContext;
    private readonly ILogger<CopyStrategyAddedEventHandler> _logger;

    public CopyStrategyAddedEventHandler(
        ProjectXTradeCopyManagerProvider provider,
        OrderEventHubDbContext dbContext,
        ILogger<CopyStrategyAddedEventHandler> logger
    )
    {
        _provider = provider;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(CopyStrategyAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CopyStrategyAddedEvent for CopyStrategyId: {CopyStrategyId}", notification.CopyStrategyId);

        try
        {
            var strategy = await _dbContext.CopyStrategies
                .Include(x => x.ProjectXAccount)
                .SingleAsync(x => x.Id == notification.CopyStrategyId, cancellationToken);

            _provider.AddManager(
                strategy.AtasAccountId,
                strategy.AtasContract,
                strategy.ProjectXAccountId,
                strategy.ProjectXContract,
                strategy.ContractMultiplier,
                strategy.ProjectXAccount.Vendor
            );

            _logger.LogInformation(
                "Successfully added manager for CopyStrategyId: {CopyStrategyId}, AtasAccountId: {AtasAccountId}, ProjectXAccountId: {ProjectXAccountId}",
                strategy.Id,
                strategy.AtasAccountId,
                strategy.ProjectXAccountId
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to find CopyStrategy with Id: {CopyStrategyId}", notification.CopyStrategyId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Manager already exists for CopyStrategyId: {CopyStrategyId}", notification.CopyStrategyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while handling CopyStrategyAddedEvent for CopyStrategyId: {CopyStrategyId}", notification.CopyStrategyId);
        }
    }
}
