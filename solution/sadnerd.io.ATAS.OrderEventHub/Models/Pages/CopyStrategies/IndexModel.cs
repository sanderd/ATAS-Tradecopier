using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.CopyStrategies;

[Authorize]
public class IndexModel : PageModel
{
    private readonly OrderEventHubDbContext _context;
    private ProjectXTradeCopyManagerProvider _managerProvider;

    public IndexModel(OrderEventHubDbContext context, ProjectXTradeCopyManagerProvider managerProvider)
    {
        _context = context;
        _managerProvider = managerProvider;
    }

    public List<CopyStrategy> CopyStrategies { get; set; } = new();
    public List<StrategyStatus> StrategyStatuses { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Fetch CopyStrategies from the database with related data
        CopyStrategies = await _context.CopyStrategies
            .Include(s => s.ProjectXAccount)
            .ThenInclude(a => a.ApiCredential)
            .ToListAsync();

        // Fetch the status of each strategy
        foreach (var strategy in CopyStrategies)
        {
            try
            {
                var manager = _managerProvider.GetManager(
                    strategy.AtasAccountId,
                    strategy.AtasContract,
                    strategy.ProjectXAccountId,
                    strategy.ProjectXContract
                );

                StrategyStatuses.Add(new StrategyStatus
                {
                    StrategyId = strategy.Id,
                    State = manager.State,
                    ConnectionStatus = manager.IsConnected() ? "Connected" : "Disconnected",
                    ErrorMessage = null
                });
            }
            catch (InvalidOperationException ex)
            {
                // Handle the error gracefully when no manager is found
                var errorMessage = "Manager not initialized";
                
                // Check if the issue is missing API credentials
                if (strategy.ProjectXAccount?.ApiCredential == null)
                {
                    errorMessage = "No API credentials assigned";
                }
                else if (strategy.ProjectXAccount.ApiCredential.IsActive == false)
                {
                    errorMessage = "API credentials inactive";
                }

                StrategyStatuses.Add(new StrategyStatus
                {
                    StrategyId = strategy.Id,
                    State = ManagerState.Error,
                    ConnectionStatus = "Not Initialized",
                    ErrorMessage = errorMessage
                });
            }
        }
    }

    public async Task<IActionResult> OnPostSetStateAsync(int strategyId, ManagerState state)
    {
        // Fetch the strategy from the database
        var strategy = await _context.CopyStrategies
            .Include(s => s.ProjectXAccount)
            .ThenInclude(a => a.ApiCredential)
            .FirstOrDefaultAsync(s => s.Id == strategyId);
            
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

            manager.SetState(state);
            return RedirectToPage();
        }
        catch (InvalidOperationException)
        {
            // Try to initialize the manager if it doesn't exist and we have valid API credentials
            if (strategy.ProjectXAccount?.ApiCredential != null && strategy.ProjectXAccount.ApiCredential.IsActive)
            {
                try
                {
                    _managerProvider.AddManager(
                        strategy.AtasAccountId,
                        strategy.AtasContract,
                        strategy.ProjectXAccountId,
                        strategy.ProjectXContract,
                        strategy.ContractMultiplier,
                        strategy.ProjectXAccount.Vendor
                    );

                    // Now try to set the state
                    var manager = _managerProvider.GetManager(
                        strategy.AtasAccountId,
                        strategy.AtasContract,
                        strategy.ProjectXAccountId,
                        strategy.ProjectXContract
                    );
                    
                    manager.SetState(state);
                    return RedirectToPage();
                }
                catch (Exception)
                {
                    // Still failed, redirect with error
                    TempData["ErrorMessage"] = $"Failed to initialize manager for strategy {strategyId}";
                    return RedirectToPage();
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Cannot initialize manager - missing or inactive API credentials for strategy {strategyId}";
                return RedirectToPage();
            }
        }
    }

    public class StrategyStatus
    {
        public int StrategyId { get; set; }
        public ManagerState State { get; set; }
        public string ConnectionStatus { get; set; } = "Unknown";
        public string? ErrorMessage { get; set; }
    }
}
