namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.ConnectionManagement;

public record TopstepConnection(
    ConnectionStatus Status,
    string SignalRConnectionKey,
    string AccountId,
    string Instrument) : ITopstepConnection
{
    public ConnectionStatus Status { get; set; } = Status;
}