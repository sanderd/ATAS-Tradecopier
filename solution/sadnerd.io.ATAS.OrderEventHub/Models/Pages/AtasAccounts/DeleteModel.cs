using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.AtasAccounts;

public class DeleteModel : PageModel
{
    private readonly TradeCopyContext _context;

    [BindProperty]
    public AtasAccount AtasAccount { get; set; }

    public DeleteModel(TradeCopyContext context)
    {
        _context = context;
    }

    public IActionResult OnGet(string id)
    {
        AtasAccount = _context.AtasAccounts.FirstOrDefault(a => a.AtasAccountId == id);

        if (AtasAccount == null)
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var account = _context.AtasAccounts.FirstOrDefault(a => a.AtasAccountId == AtasAccount.AtasAccountId);

        if (account == null)
        {
            return NotFound();
        }

        _context.AtasAccounts.Remove(account);
        _context.SaveChanges();

        return RedirectToPage("/AtasAccount/Index");
    }
}
