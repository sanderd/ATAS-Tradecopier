using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public class CopyStrategyDeletedEvent : INotification
{
    public int StrategyId { get; }
    public string AtasAccountId { get; }
    public string ProjectXAccountId { get; }
    public string AtasContract { get; }
    public string ProjectXContract { get; }
    public int ContractMultiplier { get; }

    public CopyStrategyDeletedEvent(
        int strategyId,
        string atasAccountId,
        string projectXAccountId,
        string atasContract,
        string projectXContract,
        int contractMultiplier)
    {
        StrategyId = strategyId;
        AtasAccountId = atasAccountId;
        ProjectXAccountId = projectXAccountId;
        AtasContract = atasContract;
        ProjectXContract = projectXContract;
        ContractMultiplier = contractMultiplier;
    }
}
