namespace sadnerd.io.ATAS.OrderEventHub.Data.Models;

public class ProjectXApiCredential
{
    public int Id { get; set; }
    public ProjectXVendor Vendor { get; set; }
    public string ApiKey { get; set; }
    public string ApiUser { get; set; }
    public string DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<ProjectXAccount> ProjectXAccounts { get; set; } = new List<ProjectXAccount>();
}