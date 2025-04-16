using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public class CopyStrategyDeletedEvent : INotification
{
    public int StrategyId { get; }
    public string AtasAccountId { get; }
    public string TopstepAccountId { get; }
    public string AtasContract { get; }
    public string TopstepContract { get; }
    public int ContractMultiplier { get; }

    public CopyStrategyDeletedEvent(
        int strategyId,
        string atasAccountId,
        string topstepAccountId,
        string atasContract,
        string topstepContract,
        int contractMultiplier)
    {
        StrategyId = strategyId;
        AtasAccountId = atasAccountId;
        TopstepAccountId = topstepAccountId;
        AtasContract = atasContract;
        TopstepContract = topstepContract;
        ContractMultiplier = contractMultiplier;
    }
}
