using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

public record CopyStrategyAddedEvent(int CopyStrategyId) : INotification;
