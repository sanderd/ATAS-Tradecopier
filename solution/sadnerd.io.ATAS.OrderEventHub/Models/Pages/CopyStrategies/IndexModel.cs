using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.CopyStrategies;

public class IndexModel : PageModel
{
    private readonly TradeCopyContext _context;

    public IndexModel(TradeCopyContext context)
    {
        _context = context;
    }

    public List<CopyStrategy> CopyStrategies { get; set; } = new();

    public void OnGet()
    {
        CopyStrategies = _context.CopyStrategies.ToList();
    }
}
