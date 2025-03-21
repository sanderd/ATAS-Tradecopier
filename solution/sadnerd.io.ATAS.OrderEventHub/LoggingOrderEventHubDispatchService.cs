using Microsoft.Extensions.Logging;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Messages;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;

namespace sadnerd.io.ATAS.OrderEventHub;

public class LoggingOrderEventHubDispatchService : IOrderEventHubDispatchService
{
    private readonly ILogger<LoggingOrderEventHubDispatchService> _logger;

    public LoggingOrderEventHubDispatchService(
        ILogger<LoggingOrderEventHubDispatchService> logger
    )
    {
        _logger = logger;
    }

    public void NewOrder(NewOrderEventV1Message message)
    {
        _logger.LogInformation("NewOrder: {message}", message);
    }

    public void OrderChanged(OrderChangedV1Message message)
    {
        _logger.LogInformation("OrderChanged: {message}", message);
    }

    public void PositionChanged(PositionChangedV1Message message)
    {
        _logger.LogInformation("PositionChanged: {message}", message);
    }
}