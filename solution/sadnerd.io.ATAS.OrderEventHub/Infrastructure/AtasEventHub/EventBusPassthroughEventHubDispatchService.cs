using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure.AtasEventHub;

public class EventBusPassthroughEventHubDispatchService : IOrderEventHubDispatchService
{
    private readonly IEventBus _eventBus;

    public EventBusPassthroughEventHubDispatchService(
        IEventBus eventBus
    )
    {
        _eventBus = eventBus;
    }
    public void NewOrder(NewOrderEventV1Message message)
    {
        _eventBus.PublishAsync(new NewOrderEvent(message), CancellationToken.None).GetAwaiter().GetResult();
    }

    public void OrderChanged(OrderChangedV1Message message)
    {
        _eventBus.PublishAsync(new OrderChangedEvent(message), CancellationToken.None).GetAwaiter().GetResult();
    }

    public void PositionChanged(PositionChangedV1Message message)
    {
        _eventBus.PublishAsync(new PositionChangedEvent(message), CancellationToken.None).GetAwaiter().GetResult();
    }
}