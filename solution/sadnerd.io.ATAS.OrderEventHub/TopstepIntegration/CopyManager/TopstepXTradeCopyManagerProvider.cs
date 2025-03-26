using Microsoft.Extensions.DependencyInjection;

namespace sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;

public class TopstepXTradeCopyManagerProvider
{
    private readonly IServiceScope _serviceScope;
    private List<(string atasAccountId, string instrument, string topstepAccountId, string topstepInstrument, TopstepXTradeCopyManager manager)> _managers;
    private SemaphoreSlim _managerSemaphore = new SemaphoreSlim(1);

    public TopstepXTradeCopyManagerProvider(IServiceScope serviceScope)
    {
        _serviceScope = serviceScope;
        _managers = new List<(string atasAccountId, string instrument, string topstepAccountId, string topstepInstrument, TopstepXTradeCopyManager manager)>();
    }

    public IEnumerable<TopstepXTradeCopyManager> GetManagers(string atasAccountId, string instrument)
    {
        return _managers.Where(m => m.atasAccountId == atasAccountId && m.instrument == instrument).Select(m => m.manager);
    }

    public void AddManager(string atasAccountId, string instrument, string topstepAccount, string topstepInstrument)
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

            _managers.Add((atasAccountId, instrument, topstepAccount, topstepInstrument, ActivatorUtilities.CreateInstance<TopstepXTradeCopyManager>(_serviceScope.ServiceProvider)));
        }
    }

    public IEnumerable<TopstepXTradeCopyManager> GetManagersByTopstepInformation(string topstepAccountId, string topstepInstrument)
    {
        return _managers.Where(m => m.topstepAccountId == topstepAccountId && m.topstepInstrument == topstepInstrument).Select(m => m.manager);
    }
}