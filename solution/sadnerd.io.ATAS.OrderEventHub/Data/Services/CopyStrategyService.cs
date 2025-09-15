using MediatR;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

namespace sadnerd.io.ATAS.OrderEventHub.Data.Services;

public class CopyStrategyService
{
    private readonly TradeCopyContext _context;
    private readonly IMediator _mediator;

    public CopyStrategyService(
        TradeCopyContext context, 
        IMediator mediator
    )
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task AddStrategy(
        string atasAccountId, 
        string projectXAccountId, 
        string atasContract,
        string projectXContract, 
        int contractMultiplier,
        CancellationToken cancellationToken = default
    )
    {
        var strategy = new CopyStrategy()
        {
            ProjectXAccountId = projectXAccountId, 
            AtasAccountId = atasAccountId, 
            AtasContract = atasContract,
            ProjectXContract = projectXContract, 
            ContractMultiplier = contractMultiplier
        };

        await _context.CopyStrategies.AddAsync(strategy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish the notification - no cancellation
        await _mediator.Publish(new CopyStrategyAddedEvent(strategy.Id), CancellationToken.None);
    }

    public async Task DeleteStrategy(int strategyId, CancellationToken cancellationToken = default)
    {
        var strategy = await _context.CopyStrategies.FindAsync(new object[] { strategyId }, cancellationToken);
        if (strategy == null)
        {
            throw new KeyNotFoundException($"CopyStrategy with ID {strategyId} not found.");
        }

        // Capture the original data before deletion
        var deletedEvent = new CopyStrategyDeletedEvent(
            strategy.Id,
            strategy.AtasAccountId,
            strategy.ProjectXAccountId,
            strategy.AtasContract,
            strategy.ProjectXContract,
            strategy.ContractMultiplier
        );

        _context.CopyStrategies.Remove(strategy);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish the deletion event with the original data
        await _mediator.Publish(deletedEvent, CancellationToken.None);
    }
}