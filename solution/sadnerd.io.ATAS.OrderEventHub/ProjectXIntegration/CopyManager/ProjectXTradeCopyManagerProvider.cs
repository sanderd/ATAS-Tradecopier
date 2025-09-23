using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.Factories;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;

public class ProjectXTradeCopyManagerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private List<(string atasAccountId, string instrument, string projectXAccountId, string projectXInstrument, ProjectXVendor vendor, int? apiCredentialId, ProjectXTradeCopyManager manager)> _managers;

    public ProjectXTradeCopyManagerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _managers = new List<(string atasAccountId, string instrument, string projectXAccountId, string projectXInstrument, ProjectXVendor vendor, int? apiCredentialId, ProjectXTradeCopyManager manager)>();
    }

    public IEnumerable<ProjectXTradeCopyManager> GetManagers(string atasAccountId, string instrument)
    {
        return _managers.Where(m => m.atasAccountId == atasAccountId && m.instrument == instrument).Select(m => m.manager);
    }

    public void AddManager(string atasAccountId, string instrument, string projectXAccount, string projectXInstrument, int contractMultiplier, ProjectXVendor vendor)
    {
        // Get the API credential for this ProjectX account
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderEventHubDbContext>();
        
        var account = context.ProjectXAccounts
            .Include(a => a.ApiCredential)
            .FirstOrDefault(a => a.ProjectXAccountId == projectXAccount);

        if (account?.ApiCredential == null)
        {
            throw new InvalidOperationException($"No active API credentials found for ProjectX account {projectXAccount}");
        }

        AddManagerInternal(atasAccountId, instrument, projectXAccount, projectXInstrument, contractMultiplier, vendor, account.ApiCredentialId);
    }

    public void RemoveManager(string atasAccountId, string instrument, string projectXAccountId, string projectXInstrument)
    {
        lock (_managers)
        {
            var managerToRemove = _managers.FirstOrDefault(m => 
                m.atasAccountId == atasAccountId && 
                m.instrument == instrument && 
                m.projectXAccountId == projectXAccountId && 
                m.projectXInstrument == projectXInstrument);

            if (managerToRemove.manager != null)
            {
                // Set manager to disabled state before removal
                try
                {
                    managerToRemove.manager.SetState(ManagerState.Disabled);
                }
                catch (Exception ex)
                {
                    var logger = _serviceProvider.GetService<ILogger<ProjectXTradeCopyManagerProvider>>();
                    logger?.LogWarning(ex, "Failed to disable manager before removal");
                }

                _managers.Remove(managerToRemove);
            }
        }
    }

    private void AddManagerInternal(string atasAccountId, string instrument, string projectXAccount, string projectXInstrument, int contractMultiplier, ProjectXVendor vendor, int? apiCredentialId)
    {
        if (_managers.Any(x => x.instrument == instrument && x.atasAccountId == atasAccountId && x.projectXAccountId == projectXAccount))
        {
            throw new ArgumentException("Already exists");
        }

        lock (_managers)
        {
            if (_managers.Any(x => x.instrument == instrument && x.atasAccountId == atasAccountId && x.projectXAccountId == projectXAccount))
            {
                throw new ArgumentException("Already exists");
            }

            var clientFactory = _serviceProvider.GetRequiredService<IProjectXClientFactory>();
            var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            
            IProjectXClient projectXClient;
            if (apiCredentialId.HasValue)
            {
                projectXClient = clientFactory.CreateClient(vendor, apiCredentialId.Value);
            }
            else
            {
                projectXClient = clientFactory.CreateClient(vendor);
            }

            var manager = new ProjectXTradeCopyManager(
                projectXClient,
                _serviceProvider.GetRequiredService<ILogger<ProjectXTradeCopyManager>>(),
                notificationService,
                contractMultiplier,
                projectXAccount,
                projectXInstrument
            );
            _managers.Add((atasAccountId, instrument, projectXAccount, projectXInstrument, vendor, apiCredentialId, manager));
        }
    }

    public IEnumerable<ProjectXTradeCopyManager> GetManagersByProjectXInformation(string projectXAccountId, string projectXInstrument)
    {
        return _managers.Where(m => m.projectXAccountId == projectXAccountId && m.projectXInstrument == projectXInstrument).Select(m => m.manager);
    }

    public ProjectXTradeCopyManager GetManager(string atasAccountId, string instrument, string projectXAccountId, string projectXInstrument)
    {
        var matchingManagers = _managers
            .Where(m => m.atasAccountId == atasAccountId &&
                        m.instrument == instrument &&
                        m.projectXAccountId == projectXAccountId &&
                        m.projectXInstrument == projectXInstrument)
            .ToList();

        if (matchingManagers.Count == 0)
        {
            throw new InvalidOperationException($"No matching manager found for AtasAccountId: {atasAccountId}, Instrument: {instrument}, ProjectXAccountId: {projectXAccountId}, ProjectXInstrument: {projectXInstrument}");
        }

        if (matchingManagers.Count > 1)
        {
            throw new InvalidOperationException($"Multiple matching managers found for AtasAccountId: {atasAccountId}, Instrument: {instrument}, ProjectXAccountId: {projectXAccountId}, ProjectXInstrument: {projectXInstrument}");
        }

        return matchingManagers.Single().manager;
    }
}