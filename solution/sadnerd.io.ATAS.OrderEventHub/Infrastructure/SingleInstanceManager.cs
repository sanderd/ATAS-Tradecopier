using System.Security.Principal;

namespace sadnerd.io.ATAS.OrderEventHub.Infrastructure;

public class SingleInstanceManager : IDisposable
{
    private readonly Mutex _mutex;
    private readonly string _mutexName;
    private bool _hasLock = false;
    private bool _disposed = false;

    public SingleInstanceManager(string applicationName)
    {
        // Create a unique mutex name based on the application name and user
        // This ensures different users can run the application simultaneously
        var identity = WindowsIdentity.GetCurrent();
        var userSid = identity.User?.Value ?? "Unknown";
        _mutexName = $"Global\\{applicationName}_{userSid}";

        // Create mutex - simplified for .NET 8 compatibility
        _mutex = new Mutex(false, _mutexName);
    }

    public bool TryAcquireLock(TimeSpan timeout = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SingleInstanceManager));

        if (_hasLock)
            return true;

        try
        {
            var timeoutMs = timeout == default ? 0 : (int)timeout.TotalMilliseconds;
            _hasLock = _mutex.WaitOne(timeoutMs, false);
            return _hasLock;
        }
        catch (AbandonedMutexException)
        {
            // Previous instance crashed or was terminated
            // We can safely continue as we now own the mutex
            _hasLock = true;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void ReleaseLock()
    {
        if (_hasLock && !_disposed)
        {
            try
            {
                _mutex.ReleaseMutex();
                _hasLock = false;
            }
            catch (Exception)
            {
                // Mutex may have been disposed or released already
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ReleaseLock();
            _mutex?.Dispose();
            _disposed = true;
        }
    }
}