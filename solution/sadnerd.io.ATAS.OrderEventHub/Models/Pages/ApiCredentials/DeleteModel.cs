using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.ApiCredentials;

public class DeleteModel : PageModel
{
    private readonly OrderEventHubDbContext _context;

    public DeleteModel(OrderEventHubDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ProjectXApiCredential ApiCredential { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        ApiCredential = await _context.ProjectXApiCredentials
            .Include(a => a.ProjectXAccounts)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (ApiCredential == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var apiCredential = await _context.ProjectXApiCredentials
            .Include(a => a.ProjectXAccounts)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (apiCredential != null)
        {
            // Check if any accounts are using this credential
            if (apiCredential.ProjectXAccounts.Any())
            {
                ModelState.AddModelError("", $"Cannot delete API credential. It is being used by {apiCredential.ProjectXAccounts.Count} account(s).");
                ApiCredential = apiCredential;
                return Page();
            }

            _context.ProjectXApiCredentials.Remove(apiCredential);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}