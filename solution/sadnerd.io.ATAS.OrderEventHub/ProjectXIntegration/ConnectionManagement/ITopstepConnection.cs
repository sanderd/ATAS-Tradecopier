namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.ConnectionManagement;

public interface ITopstepConnection
{
    ConnectionStatus Status { get; set; }
    string SignalRConnectionKey { get; }
    string AccountId { get; }
    string Instrument { get; }
}