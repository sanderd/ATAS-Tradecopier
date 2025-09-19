using MediatR;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Admin;

namespace sadnerd.io.ATAS.OrderEventHub.Data.Services;

public class CopyStrategyService
{
    private readonly OrderEventHubDbContext _context;
    private readonly IMediator _mediator;

    public CopyStrategyService(
        OrderEventHubDbContext context, 
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

    public async Task UpdateStrategy(
        int strategyId,
        string atasAccountId, 
        string projectXAccountId, 
        string atasContract,
        string projectXContract, 
        int contractMultiplier,
        CancellationToken cancellationToken = default
    )
    {
        var strategy = await _context.CopyStrategies
            .Include(s => s.ProjectXAccount)
            .ThenInclude(a => a.ApiCredential)
            .FirstOrDefaultAsync(s => s.Id == strategyId, cancellationToken);
            
        if (strategy == null)
        {
            throw new KeyNotFoundException($"CopyStrategy with ID {strategyId} not found.");
        }

        // Capture the original data before update for manager removal
        var originalData = new CopyStrategyDeletedEvent(
            strategy.Id,
            strategy.AtasAccountId,
            strategy.ProjectXAccountId,
            strategy.AtasContract,
            strategy.ProjectXContract,
            strategy.ContractMultiplier
        );

        // Update the strategy
        strategy.AtasAccountId = atasAccountId;
        strategy.ProjectXAccountId = projectXAccountId;
        strategy.AtasContract = atasContract;
        strategy.ProjectXContract = projectXContract;
        strategy.ContractMultiplier = contractMultiplier;

        await _context.SaveChangesAsync(cancellationToken);

        // Remove the old manager first, then add the new one
        // This ensures clean state transition
        await _mediator.Publish(originalData, CancellationToken.None);
        
        // Give a small delay to ensure the deletion handler completes
        await Task.Delay(100, cancellationToken);
        
        await _mediator.Publish(new CopyStrategyAddedEvent(strategy.Id), CancellationToken.None);
    }
}