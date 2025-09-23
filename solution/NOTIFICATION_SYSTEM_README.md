# Notification System Documentation

## Overview

This comprehensive error notification system provides real-time notifications for ProjectXTradeCopyManager errors and operations. The system supports multiple notification methods and includes a robust testing infrastructure.

## Features

### Core Infrastructure
- **Multi-sink Architecture**: Supports multiple notification destinations simultaneously
- **In-Memory Storage**: Notifications persist during runtime and survive page refreshes
- **Feature Flag System**: Testing features can be enabled/disabled via configuration
- **SignalR Real-time Updates**: Instant notification delivery to connected clients

### Notification Sinks
1. **Console Logging Sink**: Logs all notifications to console with appropriate log levels
2. **SignalR Sink**: Sends real-time notifications to web clients via SignalR

### Frontend Features
- **Bell Icon**: Notification bell in header with badge count
- **Dropdown Preview**: Quick notification preview in navigation bar
- **Standalone Page**: Comprehensive notifications page with filtering and search
- **Sound Notifications**: Different tones for different severity levels (user configurable)
- **Browser Notifications**: Desktop notifications when tab is in background
- **User Preferences**: Settings persist in localStorage

### Testing Infrastructure
- **Feature Flag Controlled**: Testing menu only visible when enabled
- **Test API Endpoints**: Send individual or bulk test notifications
- **Various Severity Levels**: Test all notification types (Info, Warning, Error, Critical)

## Configuration

### Feature Flags (appsettings.json)
```json
{
  "FeatureFlags": {
    "Testing": true,
    "NotificationTesting": true
  }
}
```

### Available Feature Flags
- `Testing`: Enables the Testing dropdown menu
- `NotificationTesting`: Enables notification testing endpoints

## API Endpoints

### Notifications API
- `GET /api/notifications`: Get notifications with optional filtering
  - Query parameters: `severity`, `since`, `count`
- `DELETE /api/notifications`: Clear all notifications

### Feature Flags API
- `GET /api/feature-flags`: Get all feature flag values
- `GET /api/feature-flags/{flagName}`: Get specific feature flag value

### Testing API (requires `NotificationTesting` feature flag)
- `POST /api/testing/notifications/send`: Send single test notification
- `POST /api/testing/notifications/send-multiple`: Send bulk test notifications

## Usage

### Accessing Notifications
1. **Bell Icon**: Click the bell icon in the header to see recent notifications
2. **Standalone Page**: Navigate to `/Notifications` for full notification management
3. **Testing Page**: Access `/Testing/Notifications` when testing features are enabled

### Notification Settings
- Click the settings button in the notifications dropdown or standalone page
- Configure sound notifications, browser notifications, and display preferences
- Settings are automatically saved to localStorage

### ProjectXTradeCopyManager Integration
The notification system is fully integrated into ProjectXTradeCopyManager and will automatically send notifications for:

- **Account Resolution**: Success/failure when connecting to ProjectX accounts
- **Contract Resolution**: Success/failure when resolving contract symbols
- **Order Operations**: Success/failure for all order types (limit, market, stop)
- **Position Management**: Stop loss and take profit updates
- **Error Conditions**: API communication errors, order mapping issues, position errors
- **State Changes**: Manager state transitions (Enabled, Disabled, Error)

### Notification Severity Levels
- **Info**: Successful operations, confirmations
- **Warning**: Non-critical issues, state changes
- **Error**: Operation failures, recoverable errors
- **Critical**: Severe errors requiring immediate attention

## Architecture

### Services
- `INotificationService`: Core notification publishing and retrieval
- `INotificationSink`: Interface for notification destinations
- `IFeatureFlagService`: Feature flag management
- `NotificationHub`: SignalR hub for real-time updates

### Frontend Components
- `notifications.js`: Core JavaScript notification manager
- `NotificationManager` class: Handles SignalR connection, audio, browser notifications
- Real-time UI updates via SignalR

### Dependencies
- **SignalR**: Real-time communication
- **Bootstrap 5**: UI components and styling
- **Font Awesome**: Icons
- **Entity Framework**: Data persistence (for copy strategies, not notifications)

## Testing

### Manual Testing
1. Enable testing features in `appsettings.json`
2. Navigate to the Testing menu ? Notification Testing
3. Send test notifications with different severity levels
4. Verify notifications appear in bell dropdown and standalone page
5. Test sound and browser notifications

### Integration Tests
Run the included integration tests to verify:
- Notification service functionality
- API endpoint responses
- Feature flag behavior
- SignalR communication

### Browser Compatibility
- **Sound Notifications**: Requires user interaction to initialize AudioContext
- **Browser Notifications**: Requires user permission
- **SignalR**: Automatically reconnects on connection loss

## Troubleshooting

### Common Issues
1. **Sounds not playing**: Ensure user has interacted with the page first
2. **Browser notifications not showing**: Check permission settings
3. **Testing menu not visible**: Verify `Testing` feature flag is enabled
4. **Notifications not updating**: Check SignalR connection status in browser console

### Debug Information
- Check browser console for SignalR connection status
- Verify feature flags via `/api/feature-flags` endpoint
- Test notification API directly via browser dev tools

## Future Enhancements

Potential improvements for the notification system:
- **Email notifications**: Add email sink for critical errors
- **Database persistence**: Store notifications in database for audit trail
- **Notification categories**: Group notifications by source/category
- **Advanced filtering**: More sophisticated filtering options
- **Export functionality**: Export notifications to CSV/JSON
- **Slack/Teams integration**: Send notifications to chat platforms