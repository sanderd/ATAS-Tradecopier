using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.ApiCredentials;

public class CreateModel : PageModel
{
    private readonly OrderEventHubDbContext _context;

    public CreateModel(OrderEventHubDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ProjectXApiCredential ApiCredential { get; set; } = new();

    public List<SelectListItem> AvailableVendors { get; set; } = new();

    public void OnGet()
    {
        LoadVendors();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadVendors();
            return Page();
        }

        ApiCredential.CreatedAt = DateTime.UtcNow;
        ApiCredential.IsActive = true;

        _context.ProjectXApiCredentials.Add(ApiCredential);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private void LoadVendors()
    {
        AvailableVendors = Enum.GetValues<ProjectXVendor>()
            .Select(v => new SelectListItem
            {
                Value = ((int)v).ToString(),
                Text = v.ToString()
            })
            .ToList();
    }
}