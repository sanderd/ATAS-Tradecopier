using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.AtasAccounts;

public class IndexModel : PageModel
{
    private readonly TradeCopyContext _context;

    public IndexModel(TradeCopyContext context)
    {
        _context = context;
    }

    public List<AtasAccount> AtasAccounts { get; set; } = new();

    public void OnGet()
    {
        // Fetch data from the database
        AtasAccounts = _context.AtasAccounts.ToList();
    }
}
