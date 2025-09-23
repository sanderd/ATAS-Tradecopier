namespace sadnerd.io.ATAS.OrderEventHub.Configuration;

public class ApplicationOptions
{
    public const string SectionName = "Application";

    public bool AllowMultipleInstances { get; set; } = false;
}