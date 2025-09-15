using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public CreateModel(TradeCopyContext context, IProjectXVendorConfigurationService vendorConfigurationService)
    {
        _context = context;
        _vendorConfigurationService = vendorConfigurationService;
    }

    public void OnGet()
    {
        var vendors = _vendorConfigurationService.GetAllVendorConfigurations();
        AvailableVendors = vendors.Select(v => new SelectListItem
        {
            Value = ((int)v.Vendor).ToString(),
            Text = v.DisplayName
        }).ToList();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            OnGet(); // Reload vendors for dropdown
            return Page();
        }

        _context.ProjectXAccounts.Add(ProjectXAccount);
        _context.SaveChanges();

        return RedirectToPage("/TopstepAccount/Index");
    }
}
