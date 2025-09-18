using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Atas;

public interface IEventBus
{
    Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent;
}