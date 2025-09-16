using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.ApiCredentials;

public class EditModel : PageModel
{
    private readonly TradeCopyContext _context;

    public EditModel(TradeCopyContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ProjectXApiCredential ApiCredential { get; set; } = new();

    public List<SelectListItem> AvailableVendors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        ApiCredential = await _context.ProjectXApiCredentials.FirstOrDefaultAsync(m => m.Id == id);

        if (ApiCredential == null)
        {
            return NotFound();
        }

        LoadVendors();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadVendors();
            return Page();
        }

        ApiCredential.UpdatedAt = DateTime.UtcNow;

        _context.Attach(ApiCredential).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ApiCredentialExists(ApiCredential.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool ApiCredentialExists(int id)
    {
        return _context.ProjectXApiCredentials.Any(e => e.Id == id);
    }

    private void LoadVendors()
    {
        AvailableVendors = Enum.GetValues<ProjectXVendor>()
            .Select(v => new SelectListItem
            {
                Value = ((int)v).ToString(),
                Text = v.ToString(),
                Selected = v == ApiCredential.Vendor
            })
            .ToList();
    }
}