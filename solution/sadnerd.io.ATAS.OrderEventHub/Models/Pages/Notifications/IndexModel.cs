using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.Notifications;

public class IndexModel : PageModel
{
    private readonly INotificationService _notificationService;

    public IndexModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public IEnumerable<Notification> Notifications { get; private set; } = new List<Notification>();

    public void OnGet()
    {
        Notifications = _notificationService.GetRecentNotifications(50);
    }
}