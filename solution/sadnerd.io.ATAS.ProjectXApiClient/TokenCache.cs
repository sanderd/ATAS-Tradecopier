using System.Text.Json.Serialization;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public class CachedToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public CachedToken(string token, TimeSpan? expirationDuration = null)
    {
        Token = token;
        // Default to 23 hours if no expiration provided (assuming 24-hour tokens with buffer)
        ExpiresAt = DateTime.UtcNow.Add(expirationDuration ?? TimeSpan.FromHours(23));
    }

    // Parameterless constructor for serialization
    public CachedToken() { }
}

public class TokenCacheKey
{
    public string ApiUrl { get; }
    public string ApiUser { get; }

    public TokenCacheKey(string apiUrl, string apiUser)
    {
        ApiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
        ApiUser = apiUser ?? throw new ArgumentNullException(nameof(apiUser));
    }

    public override bool Equals(object? obj)
    {
        return obj is TokenCacheKey other &&
               ApiUrl == other.ApiUrl &&
               ApiUser == other.ApiUser;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ApiUrl, ApiUser);
    }

    public override string ToString()
    {
        return $"{ApiUrl}|{ApiUser}";
    }
}