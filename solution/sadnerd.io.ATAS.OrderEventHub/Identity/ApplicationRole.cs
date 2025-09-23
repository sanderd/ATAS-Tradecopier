using Microsoft.AspNetCore.Identity;

namespace sadnerd.io.ATAS.OrderEventHub.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}