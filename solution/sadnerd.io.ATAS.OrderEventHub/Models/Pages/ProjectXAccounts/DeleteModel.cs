using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.ProjectXAccounts;

public class DeleteModel : PageModel
{
    private readonly OrderEventHubDbContext _context;

    [BindProperty]
    public ProjectXAccount ProjectXAccount { get; set; }

    public DeleteModel(OrderEventHubDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet(string id)
    {
        ProjectXAccount = _context.ProjectXAccounts.FirstOrDefault(a => a.ProjectXAccountId == id);

        if (ProjectXAccount == null)
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var account = _context.ProjectXAccounts.FirstOrDefault(a => a.ProjectXAccountId == ProjectXAccount.ProjectXAccountId);

        if (account == null)
        {
            return NotFound();
        }

        _context.ProjectXAccounts.Remove(account);
        _context.SaveChanges();

        return RedirectToPage("/ProjectXAccountIndex");
    }
}
