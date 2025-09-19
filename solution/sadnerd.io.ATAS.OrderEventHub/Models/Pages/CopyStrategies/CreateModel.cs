using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Services;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.CopyStrategies;

public class CreateModel : PageModel
{
    private readonly CopyStrategyService _copyStrategyService;
    private readonly OrderEventHubDbContext _context;

    public CreateModel(CopyStrategyService copyStrategyService, OrderEventHubDbContext context)
    {
        _copyStrategyService = copyStrategyService;
        _context = context;
    }

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
    public int ContractMultiplier { get; set; } = 1;

    public List<AtasAccount> AtasAccounts { get; set; } = new();
    public List<ProjectXAccount> ProjectXAccounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Fetch accounts from the database
        AtasAccounts = _context.AtasAccounts.ToList();
        ProjectXAccounts = _context.ProjectXAccounts.ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Reload accounts for form
            AtasAccounts = _context.AtasAccounts.ToList();
            ProjectXAccounts = _context.ProjectXAccounts.ToList();
            return Page();
        }

        try
        {
            await _copyStrategyService.AddStrategy(
                AtasAccountId,
                ProjectXAccountId,
                AtasContract,
                ProjectXContract,
                ContractMultiplier
            );

            TempData["SuccessMessage"] = "Strategy created successfully.";
            return RedirectToPage("/CopyStrategy/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating strategy: {ex.Message}");
            
            // Reload accounts for form
            AtasAccounts = _context.AtasAccounts.ToList();
            ProjectXAccounts = _context.ProjectXAccounts.ToList();
            return Page();
        }
    }
}