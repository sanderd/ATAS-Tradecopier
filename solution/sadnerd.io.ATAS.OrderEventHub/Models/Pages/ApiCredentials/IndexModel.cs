using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.ApiCredentials;

[Authorize]
public class IndexModel : PageModel
{
    private readonly OrderEventHubDbContext _context;

    public IndexModel(OrderEventHubDbContext context)
    {
        _context = context;
    }

    public List<ProjectXApiCredential> ApiCredentials { get; set; } = new();

    public async Task OnGetAsync()
    {
        ApiCredentials = await _context.ProjectXApiCredentials
            .OrderBy(c => c.Vendor)
            .ThenBy(c => c.DisplayName)
            .ToListAsync();
    }
}