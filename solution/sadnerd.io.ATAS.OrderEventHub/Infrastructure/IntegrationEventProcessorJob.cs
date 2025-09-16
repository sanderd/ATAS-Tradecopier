using MediatR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.Atas;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure;

internal sealed class IntegrationEventProcessorJob(
    InMemoryMessageQueue queue,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<IntegrationEventProcessorJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (IIntegrationEvent integrationEvent in
                       queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();

                IPublisher publisher = scope.ServiceProvider
                    .GetRequiredService<IPublisher>();

                await publisher.Publish(integrationEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Something went wrong!",
                    integrationEvent
                );
            }
        }
    }
}