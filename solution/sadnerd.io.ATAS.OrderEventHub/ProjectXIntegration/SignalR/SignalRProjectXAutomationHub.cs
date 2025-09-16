using MediatR;
using Microsoft.AspNetCore.SignalR;
using sadnerd.io.ATAS.OrderEventHub.IntegrationEvents.BrowserAutomation;

namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.SignalR;

public class SignalRProjectXAutomationHub : Hub
{
    private readonly IMediator _mediator;

    public SignalRProjectXAutomationHub(
        IMediator mediator
    )
    {
        _mediator = mediator;
    }

    public async Task AnnounceConnected(string accountName, string instrument)
    {
        var connectionId = Context.ConnectionId;
        await _mediator.Publish(new TopstepClientConnectedEvent(accountName, instrument, connectionId));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _mediator.Publish(new TopstepClientDisconnectedEvent(Context.ConnectionId));
        await base.OnDisconnectedAsync(exception);
    }
}