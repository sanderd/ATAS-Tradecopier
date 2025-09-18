namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.ConnectionManagement;

public class TopstepBrowserConnectionManager
{
    private List<ITopstepConnection> _connections = new();
    private object _lockObject = new();
    
    public void Add(ITopstepConnection connection)
    {
        if (_connections.All(c => c.SignalRConnectionKey != connection.SignalRConnectionKey))
        {
            lock(_lockObject)
            {
                if (_connections.All(c => c.SignalRConnectionKey != connection.SignalRConnectionKey))
                {
                    _connections.Add(connection);
                }
            }
        }
    }

    public void Heartbeat(string sessionId)
    {
        
    }

    public void Disconnect(string notificationConnectionId)
    {
        var connection = _connections.SingleOrDefault(c => c.SignalRConnectionKey == notificationConnectionId);
        if (connection != null)
        {
            connection.Status = ConnectionStatus.Disconnected;
        }
    }
}