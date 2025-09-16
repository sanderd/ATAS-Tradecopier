using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.Services;

public class TopstepTest : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CopyStrategyInitializationService> _logger;

    public TopstepTest(IServiceProvider serviceProvider, ILogger<CopyStrategyInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<IProjectXClient>();
            
            var accounts = await client.GetActiveAccounts(CancellationToken.None);
            var test = true;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CopyStrategyInitializationService is stopping.");
        return Task.CompletedTask;
    }
}