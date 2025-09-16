using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.TopstepAccounts;

public class EditModel : PageModel
{
    private readonly TradeCopyContext _context;

    public EditModel(TradeCopyContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ProjectXAccount ProjectXAccount { get; set; } = new();

    public List<SelectListItem> AvailableVendors { get; set; } = new();
    public List<SelectListItem> AvailableApiCredentials { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        ProjectXAccount = await _context.ProjectXAccounts
            .Include(a => a.ApiCredential)
            .FirstOrDefaultAsync(a => a.ProjectXAccountId == id);

        if (ProjectXAccount == null)
        {
            return NotFound();
        }

        await LoadVendorsAndCredentials(ProjectXAccount.Vendor);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadVendorsAndCredentials(ProjectXAccount.Vendor);
            return Page();
        }

        var account = await _context.ProjectXAccounts.FindAsync(ProjectXAccount.ProjectXAccountId);
        if (account == null)
        {
            return NotFound();
        }

        account.Vendor = ProjectXAccount.Vendor;
        account.ApiCredentialId = ProjectXAccount.ApiCredentialId;

        await _context.SaveChangesAsync();
        return RedirectToPage("/TopstepAccount/Index");
    }

    public async Task<JsonResult> OnGetApiCredentialsAsync(int vendorId)
    {
        var credentials = await _context.ProjectXApiCredentials
            .Where(c => c.Vendor == (ProjectXVendor)vendorId && c.IsActive)
            .Select(c => new { c.Id, c.DisplayName })
            .ToListAsync();

        return new JsonResult(credentials);
    }

    private async Task LoadVendorsAndCredentials(ProjectXVendor selectedVendor)
    {
        var vendors = Enum.GetValues<ProjectXVendor>();
        AvailableVendors = vendors.Select(v => new SelectListItem
        {
            Value = ((int)v).ToString(),
            Text = v.ToString(),
            Selected = v == selectedVendor
        }).ToList();

        var credentials = await _context.ProjectXApiCredentials
            .Where(c => c.Vendor == selectedVendor && c.IsActive)
            .ToListAsync();

        AvailableApiCredentials = credentials.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.DisplayName,
            Selected = c.Id == ProjectXAccount.ApiCredentialId
        }).ToList();
    }
}