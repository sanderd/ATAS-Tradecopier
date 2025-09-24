
# Configuration Guide

## Initial Setup

After installation, you'll need to configure the system for your trading setup.

### First-Time Setup

1. **Access the Web Interface**
   - Start the Order Event Hub application
   - Navigate to `http://localhost:15420`
   - You'll be prompted to create an admin account

2. **Create Admin Account**
   - Enter a username and password
   - This account will be used to manage the system

### ATAS Strategy Configuration

1. **Add Strategy to Chart**
   - Open ATAS and load your trading chart
   - Add the "BroadcastOrderEvents" chart strategy
    **NOTE**: The strategy only broadcasts the orders for the specific chart account / instrument. If you want to broadcast multiple accounts or instruments, you must add the strategy to each of the relevant charts.
   - Configure the strategy parameters (or leave default)

2. **Strategy Parameters**
   - **Service Host**: Usually `127.0.0.1`
   - **Service Port**: Default `35144`

### Broker Integration

The system supports various broker integrations through the Order Event Hub.

#### ProjectX/TopstepX Integration

1. **Navigate to Vendor Configuration**
   - In the web interface, go to Settings > Vendors
   - Add a new ProjectX vendor configuration

2. **Required Settings**
   - API Key
   - API User
     **NOTE**
     To find your API user:
     1. Log into the ProjectX interface of your vendor.
     2. Open Developer Tools, and copy the value for `token` from Local Storage (Firefox: F12 -> Storage -> Local Storage). It starts with 'ey'. **This token is PRIVATE, never share this with anyone, except for step 3.**
     3. Go to [jwt.io](https://www.jwt.io/) and paste the token into 'Encoded Value'
     4. Your API username is the value for the claim `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name` - see also [the following screenshot](images/jwtio.png)
     5. Log out of the ProjectX interface to invalidate your token.

3. **Account Configuration**
   - Link your trading accounts
   - Set up copy strategies
   - Configure risk parameters

### Copy Strategy Setup

1. **Create Copy Strategy**
   - Navigate to Copy Strategies
   - Click "Add New Strategy"
   - Configure source and destination accounts

2. **Strategy Parameters**
   - **Source Account**: Your ATAS trading account
   - **Destination Account**: Broker accounts to copy to
   - **Atas Contract**: The contract name as known in ATAS (eg. NQZ5)
   - **ProjectX Contract**: The contract name as known in ProjectX (eg. NQZ25)
   - **Contract Multiplier**: Multiplier applied to the copied trades. Fractional or negative values are not supported.

### Monitoring and Alerts

1. **Notification Setup**
   - Open the notification bell menu (top right)
   - Use the expand button (top right of the notification bell dropdown) to go to the notifications page
   - Open 'Settings'
	   - Enable browser notifications
	   - You can pick multiple severity levels by Ctrl-clicking each. Advised is to enable Warning, Error, Critical.

## Advanced Configuration

### Database Configuration

The system uses SQLite by default, stored in:
```
%LOCALAPPDATA%\sadnerd.tradecopy.db
```

## Configuration Files

- `appsettings.json`: Main application configuration

