using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.TopstepAccounts;

public class IndexModel : PageModel
{
    private readonly TradeCopyContext _context;

    public IndexModel(TradeCopyContext context)
    {
        _context = context;
    }

    public List<ProjectXAccount> ProjectXAccounts { get; set; } = new();

    public void OnGet()
    {
        ProjectXAccounts = _context.ProjectXAccounts.ToList();
    }
}
