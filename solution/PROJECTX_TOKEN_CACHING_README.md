# ProjectX Token Caching Implementation

## Overview

This implementation adds comprehensive token caching to the ProjectXClient system, ensuring that all instances of ProjectXClient share the same authentication token per API/ApiUser combination. This prevents multiple unnecessary authentication requests and improves performance.

## Key Features

### ? **Shared Token Cache**
- Single `IProjectXTokenCacheService` instance shared across all ProjectXClient instances
- Tokens are cached by API URL + API User combination
- Thread-safe implementation using `ConcurrentDictionary` and `SemaphoreSlim`

### ? **Automatic Token Management**
- Tokens automatically expire after 23 hours (configurable)
- Background cleanup task removes expired tokens every 30 minutes
- Failed authentication attempts invalidate cached tokens

### ? **Concurrency Control**
- Multiple simultaneous requests for the same API/User combination only result in one token request
- Double-check locking pattern prevents race conditions
- Semaphore-based synchronization for thread safety

### ? **Error Handling**
- Authentication failures automatically invalidate cached tokens
- Graceful degradation on token cache failures
- Proper exception handling and resource cleanup

## Architecture

### Core Components

#### `TokenCacheKey`
- Composite key using API URL + API User
- Proper equality and hash code implementation
- Thread-safe immutable design

#### `CachedToken`
- Stores token with expiration timestamp
- Automatic expiration checking
- Configurable expiration duration

#### `IProjectXTokenCacheService`
- Interface for token cache operations
- Diagnostic methods for testing and monitoring
- Async-first design for scalability

#### `ProjectXTokenCacheService`
- Singleton service registered in DI container
- Thread-safe concurrent operations
- Background cleanup task for expired tokens

#### `ProjectXAuthenticator`
- Updated to use shared token cache
- Double-check locking for concurrent token requests
- Proper resource disposal

### Integration Points

#### Dependency Injection
```csharp
// Registered as singleton in Startup.cs
services.AddSingleton<IProjectXTokenCacheService, ProjectXTokenCacheService>();
```

#### ProjectXClientFactory
- Injects token cache service into all created clients
- Maintains backward compatibility
- No changes required to existing client code

#### ProjectXClient
- Updated constructor to accept token cache service
- No changes to public API
- Transparent token caching for all operations

## Usage

### Basic Usage
No changes required to existing code. Token caching is transparent:

```csharp
// Existing code continues to work unchanged
var client = projectXClientFactory.CreateClient(vendor, apiCredentialId);
var accounts = await client.GetActiveAccounts();
```

### Testing and Diagnostics
New testing endpoints available when testing features are enabled:

```csharp
// Get cache status
GET /api/testing/token-cache/status

// Test token caching
POST /api/testing/token-cache/test

// Clear cached tokens
DELETE /api/testing/token-cache/clear
```

## Performance Benefits

### Before Implementation
- Each ProjectXClient instance requests its own token
- Multiple instances for same API/User = multiple token requests
- Unnecessary API calls and authentication overhead

### After Implementation
- Single token per API/User combination across all instances
- Significant reduction in authentication API calls
- Improved application startup time
- Better API rate limit compliance

## Example Scenarios

### Scenario 1: Multiple Copy Strategies
```csharp
// Before: 3 token requests
var strategy1 = new ProjectXTradeCopyManager(...); // Token request 1
var strategy2 = new ProjectXTradeCopyManager(...); // Token request 2 (same user)
var strategy3 = new ProjectXTradeCopyManager(...); // Token request 3 (same user)

// After: 1 token request
var strategy1 = new ProjectXTradeCopyManager(...); // Token request 1
var strategy2 = new ProjectXTradeCopyManager(...); // Uses cached token
var strategy3 = new ProjectXTradeCopyManager(...); // Uses cached token
```

### Scenario 2: Concurrent Access
```csharp
// Multiple threads requesting tokens simultaneously
// Before: Potentially N token requests
// After: Only 1 token request, others wait and use cached result
```

## Configuration

### Default Settings
- Token expiration: 23 hours (configurable)
- Cleanup interval: 30 minutes
- Cache size: Unlimited (expired tokens auto-cleaned)

### Customization
```csharp
// Custom expiration when setting tokens
await tokenCache.SetTokenAsync(apiUrl, apiUser, token, TimeSpan.FromHours(12));
```

## Testing

### Manual Testing
Access the testing page at `/Testing/Notifications` when testing features are enabled:

1. **Cache Status**: View current cached tokens
2. **Test Caching**: Create test tokens and verify retrieval
3. **Clear Cache**: Remove specific or expired tokens
4. **Monitor**: Watch real-time notifications for cache operations

### API Testing
```bash
# Get cache status
curl GET "localhost:15420/api/testing/token-cache/status"

# Test token caching
curl -X POST "localhost:15420/api/testing/token-cache/test" \
  -H "Content-Type: application/json" \
  -d '{"apiUrl":"https://api.test.com","apiUser":"testuser","expirationMinutes":60}'

# Clear specific token
curl -X DELETE "localhost:15420/api/testing/token-cache/clear?apiUrl=https://api.test.com&apiUser=testuser"
```

## Monitoring

### Real-time Notifications
Token cache operations generate notifications visible in the notification system:
- Token cache hits/misses
- Token creation and expiration
- Cache cleanup operations
- Error conditions

### Diagnostic Methods
```csharp
// Available on IProjectXTokenCacheService
int GetCachedTokenCount();
IEnumerable<string> GetCachedTokenKeys();
```

## Security Considerations

### Token Storage
- Tokens stored in memory only (not persisted)
- Automatic cleanup of expired tokens
- No sensitive data logged

### Access Control
- Testing endpoints protected by feature flags
- Production deployments should disable testing features
- Tokens isolated by API URL + User combination

## Migration Notes

### Backward Compatibility
- ? No breaking changes to existing code
- ? Existing ProjectXClient usage unchanged
- ? Transparent upgrade path

### Deployment
- ? Safe to deploy without coordination
- ? Gradual performance improvement as cache warms up
- ? No database schema changes required

## Future Enhancements

### Potential Improvements
- **Persistent Cache**: Store tokens in Redis for multi-instance scenarios
- **Advanced Metrics**: Detailed cache hit/miss analytics
- **Token Refresh**: Proactive token renewal before expiration
- **Health Checks**: Monitor cache health and performance
- **Configuration**: Runtime configuration changes

### Monitoring Integration
- Integration with application performance monitoring
- Custom metrics for cache effectiveness
- Alerting for authentication failures

## Troubleshooting

### Common Issues

#### Cache Not Working
- Verify `IProjectXTokenCacheService` is registered as singleton
- Check feature flags are enabled for testing
- Ensure ProjectXClientFactory is injecting cache service

#### Multiple Token Requests
- Verify same API URL + User combination
- Check for race conditions in client creation
- Monitor cache hit rates through testing endpoints

#### Memory Usage
- Background cleanup runs every 30 minutes
- Manual cleanup available via testing endpoints
- Monitor cache size through diagnostic methods

### Debug Information
- Enable testing features to access cache diagnostics
- Monitor notifications for cache operations
- Use API endpoints to inspect cache state

This implementation provides a robust, scalable, and maintainable solution for ProjectX token caching while maintaining full backward compatibility and adding comprehensive testing and monitoring capabilities.
