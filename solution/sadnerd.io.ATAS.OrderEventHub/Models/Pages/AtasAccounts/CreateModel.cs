using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.AtasAccounts;

public class CreateModel : PageModel
{
    private readonly OrderEventHubDbContext _context;

    [BindProperty]
    public AtasAccount AtasAccount { get; set; }

    public CreateModel(OrderEventHubDbContext context)
    {
        _context = context;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.AtasAccounts.Add(AtasAccount);
        _context.SaveChanges();

        return RedirectToPage("/AtasAccount/Index");
    }
}
