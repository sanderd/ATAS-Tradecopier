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
        string topstepAccountId, 
        string atasContract,
        string topstepContract, 
        int contractMultiplier,
        CancellationToken cancellationToken = default
    )
    {
        var strategy = new CopyStrategy()
        {
            TopstepAccountId = topstepAccountId, 
            AtasAccountId = atasAccountId, 
            AtasContract = atasContract,
            TopstepContract = topstepContract, 
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

        _context.CopyStrategies.Remove(strategy);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish the deletion event
        await _mediator.Publish(new CopyStrategyDeletedEvent(strategyId), CancellationToken.None);
    }
}