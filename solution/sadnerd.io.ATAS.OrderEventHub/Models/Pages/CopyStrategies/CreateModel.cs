using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Services;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.CopyStrategies;

public class CreateModel : PageModel
{
    private readonly CopyStrategyService _copyStrategyService;
    private readonly TradeCopyContext _context;

    public CreateModel(CopyStrategyService copyStrategyService, TradeCopyContext context)
    {
        _copyStrategyService = copyStrategyService;
        _context = context;
    }

    [BindProperty]
    public string AtasAccountId { get; set; }

    [BindProperty]
    public string TopstepAccountId { get; set; }

    [BindProperty]
    public string AtasContract { get; set; }

    [BindProperty]
    public string TopstepContract { get; set; }

    [BindProperty]
    public int ContractMultiplier { get; set; }

    public List<AtasAccount> AtasAccounts { get; set; } = new();
    public List<TopstepAccount> TopstepAccounts { get; set; } = new();

    public void OnGet()
    {
        // Fetch accounts from the database
        AtasAccounts = _context.AtasAccounts.ToList();
        TopstepAccounts = _context.TopstepAccount.ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _copyStrategyService.AddStrategy(
            AtasAccountId,
            TopstepAccountId,
            AtasContract,
            TopstepContract,
            ContractMultiplier
        );

        return RedirectToPage("/CopyStrategy/Index");
    }
}