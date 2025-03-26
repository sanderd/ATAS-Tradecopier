using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents;

internal sealed class EventBus : IEventBus
{
    private readonly ILogger<EventBus> _logger;
    private readonly InMemoryMessageQueue _queue;

    public EventBus(
        ILogger<EventBus> logger,
        InMemoryMessageQueue queue
    )
    {
        _logger = logger;
        _queue = queue;
    }

    public async Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent
    {
        _logger.LogDebug("Received eventbus event: {0}", JsonConvert.SerializeObject(integrationEvent));
        await _queue.Writer.WriteAsync(integrationEvent, cancellationToken);
    }
}