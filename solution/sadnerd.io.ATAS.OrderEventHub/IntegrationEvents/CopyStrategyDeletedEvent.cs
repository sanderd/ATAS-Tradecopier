using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public class CopyStrategyDeletedEvent : INotification
{
    public int StrategyId { get; }

    public CopyStrategyDeletedEvent(int strategyId)
    {
        StrategyId = strategyId;
    }
}
