using HttpTracer;
using HttpTracer.Logger;
using RestSharp;
using RestSharp.Authenticators;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public class ProjectXAuthenticator : AuthenticatorBase, IDisposable
{
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _username;
    private readonly IProjectXTokenCacheService _tokenCache;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private bool _disposed = false;

    public ProjectXAuthenticator(
        string baseUrl, 
        string apiKey, 
        string username, 
        IProjectXTokenCacheService tokenCache) : base("")
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _username = username;
        _tokenCache = tokenCache;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
    {
        // First try to get from cache
        var cachedToken = await _tokenCache.GetTokenAsync(_baseUrl, _username);
        if (!string.IsNullOrEmpty(cachedToken))
        {
            Token = cachedToken;
        }
        else
        {
            // If no cached token, acquire new one with locking to prevent concurrent requests
            await _tokenLock.WaitAsync();
            try
            {
                // Double-check pattern: another thread might have acquired the token while we were waiting
                cachedToken = await _tokenCache.GetTokenAsync(_baseUrl, _username);
                if (!string.IsNullOrEmpty(cachedToken))
                {
                    Token = cachedToken;
                }
                else
                {
                    // Actually get a new token
                    Token = await GetToken();
                    if (!string.IsNullOrEmpty(Token))
                    {
                        // Cache the new token
                        await _tokenCache.SetTokenAsync(_baseUrl, _username, Token);
                    }
                }
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        return new HeaderParameter(KnownHeaders.Authorization, "Bearer " + Token);
    }

    private async Task<string?> GetToken()
    {
        var options = new RestClientOptions(_baseUrl)
        {
            ConfigureMessageHandler = handler => new HttpTracerHandler(handler, new ConsoleLogger(), HttpMessageParts.All)
        };
        
        using var client = new RestClient(options);

        var request = new RestRequest("api/Auth/loginKey")
            .AddJsonBody(new
            {
                userName = _username,
                apiKey = _apiKey
            });
        request.AddHeader("Accept", "text/plain");

        try
        {
            var response = await client.PostAsync<TokenResponse>(request);
            
            if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
            {
                return response.Token;
            }
            else
            {
                // If authentication failed, invalidate any cached token
                await _tokenCache.InvalidateTokenAsync(_baseUrl, _username);
                throw new InvalidOperationException($"Authentication failed: {response?.ErrorMessage ?? "Unknown error"}");
            }
        }
        catch (Exception ex)
        {
            // On any error, invalidate cached token to force refresh on next attempt
            await _tokenCache.InvalidateTokenAsync(_baseUrl, _username);
            throw new InvalidOperationException($"Failed to obtain authentication token: {ex.Message}", ex);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _tokenLock?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}