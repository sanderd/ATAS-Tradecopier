using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.TopstepAccounts;

public class DeleteModel : PageModel
{
    private readonly TradeCopyContext _context;

    [BindProperty]
    public TopstepAccount TopstepAccount { get; set; }

    public DeleteModel(TradeCopyContext context)
    {
        _context = context;
    }

    public IActionResult OnGet(string id)
    {
        TopstepAccount = _context.TopstepAccount.FirstOrDefault(a => a.TopstepAccountId == id);

        if (TopstepAccount == null)
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var account = _context.TopstepAccount.FirstOrDefault(a => a.TopstepAccountId == TopstepAccount.TopstepAccountId);

        if (account == null)
        {
            return NotFound();
        }

        _context.TopstepAccount.Remove(account);
        _context.SaveChanges();

        return RedirectToPage("/TopstepAccount/Index");
    }
}
