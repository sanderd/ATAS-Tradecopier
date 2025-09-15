using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.Factories;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

public class ProjectXTradeCopyManagerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private List<(string atasAccountId, string instrument, string projectXAccountId, string projectXInstrument, ProjectXVendor vendor, ProjectXTradeCopyManager manager)> _managers;

    public ProjectXTradeCopyManagerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _managers = new List<(string atasAccountId, string instrument, string projectXAccountId, string projectXInstrument, ProjectXVendor vendor, ProjectXTradeCopyManager manager)>();
    }

    public IEnumerable<ProjectXTradeCopyManager> GetManagers(string atasAccountId, string instrument)
    {
        return _managers.Where(m => m.atasAccountId == atasAccountId && m.instrument == instrument).Select(m => m.manager);
    }

    public void AddManager(string atasAccountId, string instrument, string projectXAccount, string projectXInstrument, int contractMultiplier, ProjectXVendor vendor)
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
            var projectXClient = clientFactory.CreateClient(vendor);

            var manager = new ProjectXTradeCopyManager(
                projectXClient,
                _serviceProvider.GetRequiredService<ILogger<ProjectXTradeCopyManager>>(),
                contractMultiplier,
                projectXAccount,
                projectXInstrument
            );
            _managers.Add((atasAccountId, instrument, projectXAccount, projectXInstrument, vendor, manager));
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