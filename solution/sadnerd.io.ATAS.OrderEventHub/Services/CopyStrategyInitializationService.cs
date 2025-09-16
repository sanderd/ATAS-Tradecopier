using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

namespace sadnerd.io.ATAS.OrderEventHub.Services;

public class CopyStrategyInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CopyStrategyInitializationService> _logger;

    public CopyStrategyInitializationService(IServiceProvider serviceProvider, ILogger<CopyStrategyInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting CopyStrategy initialization...");

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TradeCopyContext>();
            var managerProvider = scope.ServiceProvider.GetRequiredService<ProjectXTradeCopyManagerProvider>();

            var strategies = await context.CopyStrategies
                .Include(x => x.ProjectXAccount)
                .ThenInclude(a => a.ApiCredential)
                .ToListAsync(cancellationToken);

            _logger.LogInformation($"Found {strategies.Count} copy strategies to initialize.");

            foreach (var strategy in strategies)
            {
                try
                {
                    // Check if the strategy has valid API credentials before trying to initialize
                    if (strategy.ProjectXAccount?.ApiCredential == null)
                    {
                        _logger.LogWarning($"Strategy {strategy.Id} skipped - no API credentials assigned to ProjectX account {strategy.ProjectXAccountId}");
                        continue;
                    }

                    if (!strategy.ProjectXAccount.ApiCredential.IsActive)
                    {
                        _logger.LogWarning($"Strategy {strategy.Id} skipped - API credentials are inactive for ProjectX account {strategy.ProjectXAccountId}");
                        continue;
                    }

                    managerProvider.AddManager(
                        strategy.AtasAccountId,
                        strategy.AtasContract,
                        strategy.ProjectXAccountId,
                        strategy.ProjectXContract,
                        strategy.ContractMultiplier,
                        strategy.ProjectXAccount.Vendor
                    );

                    _logger.LogInformation($"Successfully initialized manager for strategy {strategy.Id}.");
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning($"Manager for strategy {strategy.Id} already exists: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, $"Failed to initialize manager for strategy {strategy.Id} - missing API credentials or configuration issue.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error while initializing manager for strategy {strategy.Id}.");
                }
            }
        }

        _logger.LogInformation("CopyStrategy initialization completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CopyStrategyInitializationService is stopping.");
        return Task.CompletedTask;
    }
}