namespace sadnerd.io.ATAS.OrderEventHub.Configuration;

public class ServiceWireOptions
{
    public const string SectionName = "ServiceWire";

    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 35144;
}