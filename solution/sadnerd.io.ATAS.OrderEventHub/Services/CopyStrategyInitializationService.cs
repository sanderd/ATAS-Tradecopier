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
            var managerProvider = scope.ServiceProvider.GetRequiredService<TopstepXTradeCopyManagerProvider>();

            var strategies = context.CopyStrategies.ToList();

            foreach (var strategy in strategies)
            {
                try
                {
                    managerProvider.AddManager(
                        strategy.AtasAccountId,
                        strategy.AtasContract,
                        strategy.TopstepAccountId,
                        strategy.TopstepContract,
                        strategy.ContractMultiplier
                    );

                    _logger.LogInformation($"Initialized manager for strategy {strategy.Id}.");
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning($"Manager for strategy {strategy.Id} already exists: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to initialize manager for strategy {strategy.Id}.");
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
