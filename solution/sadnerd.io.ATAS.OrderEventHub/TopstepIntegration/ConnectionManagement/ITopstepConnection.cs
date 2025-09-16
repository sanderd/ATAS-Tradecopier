namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.ConnectionManagement;

public interface ITopstepConnection
{
    ConnectionStatus Status { get; set; }
    string SignalRConnectionKey { get; }
    string AccountId { get; }
    string Instrument { get; }
}