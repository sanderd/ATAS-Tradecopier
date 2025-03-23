using Microsoft.AspNetCore.SignalR;

namespace sadnerd.io.ATAS.OrderEventHub;

public class SignalRTopstepAutomationHub : Hub
{
    public void Send(string name, string message)
    {
        //Clients.All.Send(name, message);
    }

}