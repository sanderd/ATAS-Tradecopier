using MediatR;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.CommandHandlers;

public class CopyStrategyAddedEventHandler : INotificationHandler<CopyStrategyAddedEvent>
{
    private readonly TopstepXTradeCopyManagerProvider _provider;
    private readonly TradeCopyContext _dbContext;
    private readonly ILogger<CopyStrategyAddedEventHandler> _logger;

    public CopyStrategyAddedEventHandler(
        TopstepXTradeCopyManagerProvider provider,
        TradeCopyContext dbContext,
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
            var strategy = await _dbContext.CopyStrategies.SingleAsync(
                x => x.Id == notification.CopyStrategyId,
                cancellationToken
            );

            _provider.AddManager(
                strategy.AtasAccountId,
                strategy.AtasContract,
                strategy.TopstepAccountId,
                strategy.TopstepContract,
                strategy.ContractMultiplier
            );

            _logger.LogInformation(
                "Successfully added manager for CopyStrategyId: {CopyStrategyId}, AtasAccountId: {AtasAccountId}, TopstepAccountId: {TopstepAccountId}",
                strategy.Id,
                strategy.AtasAccountId,
                strategy.TopstepAccountId
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
