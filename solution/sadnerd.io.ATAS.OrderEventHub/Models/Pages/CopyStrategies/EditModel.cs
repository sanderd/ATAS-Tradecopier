using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Services;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;
using System.ComponentModel.DataAnnotations;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.CopyStrategies;

public class EditModel : PageModel
{
    private readonly CopyStrategyService _copyStrategyService;
    private readonly OrderEventHubDbContext _context;
    private readonly ProjectXTradeCopyManagerProvider _managerProvider;

    public EditModel(
        CopyStrategyService copyStrategyService, 
        OrderEventHubDbContext context,
        ProjectXTradeCopyManagerProvider managerProvider)
    {
        _copyStrategyService = copyStrategyService;
        _context = context;
        _managerProvider = managerProvider;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Atas Account is required")]
    [Display(Name = "Atas Account")]
    public string AtasAccountId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "ProjectX Account is required")]
    [Display(Name = "ProjectX Account")]
    public string ProjectXAccountId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Atas Contract is required")]
    [Display(Name = "Atas Contract")]
    public string AtasContract { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "ProjectX Contract is required")]
    [Display(Name = "ProjectX Contract")]
    public string ProjectXContract { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Contract Multiplier is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Contract Multiplier must be at least 1")]
    [Display(Name = "Contract Multiplier")]
    public int ContractMultiplier { get; set; }

    public List<AtasAccount> AtasAccounts { get; set; } = new();
    public List<ProjectXAccount> ProjectXAccounts { get; set; } = new();
    public bool IsStrategyActive { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var strategy = await _context.CopyStrategies
            .Include(s => s.ProjectXAccount)
            .ThenInclude(a => a.ApiCredential)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (strategy == null)
        {
            return NotFound();
        }

        // Check if strategy is active (has an active manager)
        try
        {
            var manager = _managerProvider.GetManager(
                strategy.AtasAccountId,
                strategy.AtasContract,
                strategy.ProjectXAccountId,
                strategy.ProjectXContract
            );
            IsStrategyActive = manager.State == ManagerState.Enabled;
        }
        catch (InvalidOperationException)
        {
            IsStrategyActive = false;
        }

        // If strategy is active, prevent editing
        if (IsStrategyActive)
        {
            TempData["ErrorMessage"] = "Cannot edit an active strategy. Please disable it first.";
            return RedirectToPage("/CopyStrategy/Index");
        }

        // Populate form fields
        Id = strategy.Id;
        AtasAccountId = strategy.AtasAccountId;
        ProjectXAccountId = strategy.ProjectXAccountId;
        AtasContract = strategy.AtasContract;
        ProjectXContract = strategy.ProjectXContract;
        ContractMultiplier = strategy.ContractMultiplier;

        // Fetch accounts from the database
        AtasAccounts = await _context.AtasAccounts.ToListAsync();
        ProjectXAccounts = await _context.ProjectXAccounts.ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Reload accounts for form
            AtasAccounts = await _context.AtasAccounts.ToListAsync();
            ProjectXAccounts = await _context.ProjectXAccounts.ToListAsync();
            return Page();
        }

        // Double-check that strategy is not active before updating
        var strategy = await _context.CopyStrategies.FindAsync(Id);
        if (strategy == null)
        {
            return NotFound();
        }

        try
        {
            var manager = _managerProvider.GetManager(
                strategy.AtasAccountId,
                strategy.AtasContract,
                strategy.ProjectXAccountId,
                strategy.ProjectXContract
            );
            if (manager.State == ManagerState.Enabled)
            {
                TempData["ErrorMessage"] = "Cannot edit an active strategy. Please disable it first.";
                return RedirectToPage("/CopyStrategy/Index");
            }
        }
        catch (InvalidOperationException)
        {
            // Manager doesn't exist, safe to edit
        }

        try
        {
            await _copyStrategyService.UpdateStrategy(
                Id,
                AtasAccountId,
                ProjectXAccountId,
                AtasContract,
                ProjectXContract,
                ContractMultiplier
            );

            TempData["SuccessMessage"] = "Strategy updated successfully.";
            return RedirectToPage("/CopyStrategy/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error updating strategy: {ex.Message}");
            
            // Reload accounts for form
            AtasAccounts = await _context.AtasAccounts.ToListAsync();
            ProjectXAccounts = await _context.ProjectXAccounts.ToListAsync();
            return Page();
        }
    }
}