using MediatR;

namespace sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Admin;

public record CopyStrategyAddedEvent(int CopyStrategyId) : INotification;
