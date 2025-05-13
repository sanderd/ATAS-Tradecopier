using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;
using sadnerd.io.ATAS.ProjectXApiClient;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

public class TopstepXTradeCopyManagerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private List<(string atasAccountId, string instrument, string topstepAccountId, string topstepInstrument, TopstepXTradeCopyManager manager)> _managers;

    public TopstepXTradeCopyManagerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _managers = new List<(string atasAccountId, string instrument, string topstepAccountId, string topstepInstrument, TopstepXTradeCopyManager manager)>();
    }

    public IEnumerable<TopstepXTradeCopyManager> GetManagers(string atasAccountId, string instrument)
    {
        return _managers.Where(m => m.atasAccountId == atasAccountId && m.instrument == instrument).Select(m => m.manager);
    }

    public void AddManager(string atasAccountId, string instrument, string topstepAccount, string topstepInstrument, int contractMultiplier)
    {
        if (_managers.Any(x => x.instrument == instrument && x.atasAccountId == atasAccountId && x.topstepAccountId == topstepAccount))
        {
            throw new ArgumentException("Already exists");
        }

        lock (_managers)
        {
            if (_managers.Any(x => x.instrument == instrument && x.atasAccountId == atasAccountId && x.topstepAccountId == topstepAccount))
            {
                throw new ArgumentException("Already exists");
            }

            var manager = new TopstepXTradeCopyManager(
                _serviceProvider.GetRequiredService<ITopstepBrowserAutomationClient>(),
                _serviceProvider.GetRequiredService<IProjectXClient>(),
                _serviceProvider.GetRequiredService<ILogger<TopstepXTradeCopyManager>>(),
                contractMultiplier,
                topstepAccount,
                topstepInstrument
            );
            _managers.Add((atasAccountId, instrument, topstepAccount, topstepInstrument, manager));
        }
    }

    public IEnumerable<TopstepXTradeCopyManager> GetManagersByTopstepInformation(string topstepAccountId, string topstepInstrument)
    {
        return _managers.Where(m => m.topstepAccountId == topstepAccountId && m.topstepInstrument == topstepInstrument).Select(m => m.manager);
    }

    public TopstepXTradeCopyManager GetManager(string atasAccountId, string instrument, string topstepAccountId, string topstepInstrument)
    {
        var matchingManagers = _managers
            .Where(m => m.atasAccountId == atasAccountId &&
                        m.instrument == instrument &&
                        m.topstepAccountId == topstepAccountId &&
                        m.topstepInstrument == topstepInstrument)
            .ToList();

        if (matchingManagers.Count == 0)
        {
            throw new InvalidOperationException($"No matching manager found for AtasAccountId: {atasAccountId}, Instrument: {instrument}, TopstepAccountId: {topstepAccountId}, TopstepInstrument: {topstepInstrument}");
        }

        if (matchingManagers.Count > 1)
        {
            throw new InvalidOperationException($"Multiple matching managers found for AtasAccountId: {atasAccountId}, Instrument: {instrument}, TopstepAccountId: {topstepAccountId}, TopstepInstrument: {topstepInstrument}");
        }

        return matchingManagers.Single().manager;
    }
}