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

    public List<TopstepAccount> TopstepAccounts { get; set; } = new();

    public void OnGet()
    {
        TopstepAccounts = _context.TopstepAccount.ToList();
    }
}
