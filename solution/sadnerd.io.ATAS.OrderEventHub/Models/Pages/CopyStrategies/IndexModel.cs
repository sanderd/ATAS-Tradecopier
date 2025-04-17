using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.Models.Pages.CopyStrategies;

public class IndexModel : PageModel
{
    private readonly TradeCopyContext _context;
    private TopstepXTradeCopyManagerProvider _managerProvider;

    public IndexModel(TradeCopyContext context, TopstepXTradeCopyManagerProvider managerProvider)
    {
        _context = context;
        _managerProvider = managerProvider;
    }

    public List<CopyStrategy> CopyStrategies { get; set; } = new();
    public List<StrategyStatus> StrategyStatuses { get; set; } = new();

    public void OnGet()
    {
        // Fetch CopyStrategies from the database
        CopyStrategies = _context.CopyStrategies.ToList();

        // Fetch the status of each strategy
        foreach (var strategy in CopyStrategies)
        {
            try
            {
                var manager = _managerProvider.GetManager(
                    strategy.AtasAccountId,
                    strategy.AtasContract,
                    strategy.TopstepAccountId,
                    strategy.TopstepContract
                );

                StrategyStatuses.Add(new StrategyStatus
                {
                    StrategyId = strategy.Id,
                    ErrorState = manager.ErrorState,
                    ConnectionStatus = manager.IsConnected() ? "Connected" : "Disconnected" // Assuming this property exists
                });
            }
            catch (InvalidOperationException)
            {
                // Log or handle the error if no matching manager is found
                StrategyStatuses.Add(new StrategyStatus
                {
                    StrategyId = strategy.Id,
                    ErrorState = null, // Indicate that the manager is not running
                    ConnectionStatus = "Disconnected" // Default to disconnected
                });
            }
        }
    }
    public IActionResult OnPostClearErrorState(int strategyId)
    {
        var strategy = CopyStrategies.FirstOrDefault(s => s.Id == strategyId);
        if (strategy == null)
        {
            return NotFound();
        }

        try
        {
            var manager = _managerProvider.GetManager(
                strategy.AtasAccountId,
                strategy.AtasContract,
                strategy.TopstepAccountId,
                strategy.TopstepContract
            );

            manager.ClearErrorState(); // Clear the error state
            return RedirectToPage(); // Refresh the page
        }
        catch (InvalidOperationException)
        {
            // Handle the case where the manager is not found
            return NotFound();
        }
    }

    public class StrategyStatus
    {
        public int StrategyId { get; set; }
        public bool? ErrorState { get; set; } // Null indicates the manager is not running
        public string ConnectionStatus { get; set; } = "Unknown"; // Default to unknown
    }
}
