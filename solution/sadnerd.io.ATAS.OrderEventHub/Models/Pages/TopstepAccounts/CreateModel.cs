using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.TopstepAccounts;

public class CreateModel : PageModel
{
    private readonly TradeCopyContext _context;

    [BindProperty]
    public TopstepAccount TopstepAccount { get; set; }

    public CreateModel(TradeCopyContext context)
    {
        _context = context;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.TopstepAccount.Add(TopstepAccount);
        _context.SaveChanges();

        return RedirectToPage("/TopstepAccount/Index");
    }
}
