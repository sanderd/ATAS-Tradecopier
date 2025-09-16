namespace sadnerd.io.ATAS.OrderEventHub.Data.Models;

public class ProjectXAccount
{
    public string ProjectXAccountId { get; set; }
    public ProjectXVendor Vendor { get; set; }
    public int? ApiCredentialId { get; set; }
    
    // Navigation properties
    public ProjectXApiCredential? ApiCredential { get; set; }
}