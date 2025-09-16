using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.Services;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.TopstepAccounts;

public class CreateModel : PageModel
{
    private readonly TradeCopyContext _context;
    private readonly IProjectXVendorConfigurationService _vendorConfigurationService;

    [BindProperty]
    public ProjectXAccount ProjectXAccount { get; set; }
    
    public List<SelectListItem> AvailableVendors { get; set; } = new();
    public List<SelectListItem> AvailableApiCredentials { get; set; } = new();

    public CreateModel(TradeCopyContext context, IProjectXVendorConfigurationService vendorConfigurationService)
    {
        _context = context;
        _vendorConfigurationService = vendorConfigurationService;
    }

    public async Task OnGetAsync()
    {
        await LoadVendorsAndCredentials();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadVendorsAndCredentials();
            return Page();
        }

        _context.ProjectXAccounts.Add(ProjectXAccount);
        await _context.SaveChangesAsync();

        return RedirectToPage("/TopstepAccount/Index");
    }

    public async Task<IActionResult> OnGetApiCredentialsAsync(int vendorId)
    {
        var credentials = await _context.ProjectXApiCredentials
            .Where(c => c.Vendor == (ProjectXVendor)vendorId && c.IsActive)
            .Select(c => new { c.Id, c.DisplayName })
            .ToListAsync();

        return new JsonResult(credentials);
    }

    private async Task LoadVendorsAndCredentials()
    {
        var vendors = Enum.GetValues<ProjectXVendor>();
        AvailableVendors = vendors.Select(v => new SelectListItem
        {
            Value = ((int)v).ToString(),
            Text = v.ToString()
        }).ToList();

        // Load all active API credentials for the dropdown
        var allCredentials = await _context.ProjectXApiCredentials
            .Where(c => c.IsActive)
            .ToListAsync();

        AvailableApiCredentials = allCredentials.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Vendor} - {c.DisplayName}",
            Group = new SelectListGroup { Name = c.Vendor.ToString() }
        }).ToList();
    }
}
