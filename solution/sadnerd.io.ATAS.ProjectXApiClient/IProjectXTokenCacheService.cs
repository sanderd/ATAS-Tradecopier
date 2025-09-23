using System.Collections.Concurrent;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public interface IProjectXTokenCacheService
{
    Task<string?> GetTokenAsync(string apiUrl, string apiUser);
    Task SetTokenAsync(string apiUrl, string apiUser, string token, TimeSpan? expiration = null);
    Task InvalidateTokenAsync(string apiUrl, string apiUser);
    void ClearExpiredTokens();
    
    // Diagnostic methods for testing
    int GetCachedTokenCount();
    IEnumerable<string> GetCachedTokenKeys();
}

public class ProjectXTokenCacheService : IProjectXTokenCacheService
{
    private readonly ConcurrentDictionary<TokenCacheKey, CachedToken> _tokenCache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public ProjectXTokenCacheService()
    {
        // Start background cleanup task
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    ClearExpiredTokens();
                    await Task.Delay(TimeSpan.FromMinutes(30)); // Cleanup every 30 minutes
                }
                catch (Exception)
                {
                    // Silently ignore errors during cleanup for now
                    // TODO: Add proper logging when available
                }
            }
        });
    }

    public async Task<string?> GetTokenAsync(string apiUrl, string apiUser)
    {
        var key = new TokenCacheKey(apiUrl, apiUser);
        
        if (_tokenCache.TryGetValue(key, out var cachedToken))
        {
            if (!cachedToken.IsExpired)
            {
                return cachedToken.Token;
            }
            
            // Token is expired, remove it
            await InvalidateTokenAsync(apiUrl, apiUser);
        }

        return null;
    }

    public async Task SetTokenAsync(string apiUrl, string apiUser, string token, TimeSpan? expiration = null)
    {
        var key = new TokenCacheKey(apiUrl, apiUser);
        var cachedToken = new CachedToken(token, expiration);

        await _cacheLock.WaitAsync();
        try
        {
            _tokenCache.AddOrUpdate(key, cachedToken, (_, _) => cachedToken);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task InvalidateTokenAsync(string apiUrl, string apiUser)
    {
        var key = new TokenCacheKey(apiUrl, apiUser);
        
        await _cacheLock.WaitAsync();
        try
        {
            _tokenCache.TryRemove(key, out _);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public void ClearExpiredTokens()
    {
        var expiredKeys = _tokenCache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _tokenCache.TryRemove(key, out _);
        }
    }

    public int GetCachedTokenCount() => _tokenCache.Count;
    
    public IEnumerable<string> GetCachedTokenKeys() => _tokenCache.Keys.Select(k => k.ToString());
}