using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.ProjectXAccounts;

[Authorize]
public class IndexModel : PageModel
{
    private readonly OrderEventHubDbContext _context;

    public IndexModel(OrderEventHubDbContext context)
    {
        _context = context;
    }

    public List<ProjectXAccount> ProjectXAccounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        ProjectXAccounts = await _context.ProjectXAccounts
            .Include(p => p.ApiCredential)
            .ToListAsync();
    }
}
