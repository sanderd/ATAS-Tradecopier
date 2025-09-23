

# Installation Guide

## Prerequisites

- **Windows 10/11**
- **ATAS Platform installed and launched at least once**
  - The build script requires the ATAS Strategies folder to exist
  - This folder is created when ATAS is first launched
- **.NET 8.0 SDK** (automatically installed by build script if missing)

## Quick Installation

1. **Install ATAS Platform**
   - Download and install ATAS from [https://atas.net](https://atas.net)
   - **Important:** Launch ATAS at least once to create the user folders
   - You can close ATAS after the initial launch

2. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd sadnerd.io.atas.tradecopy
   ```

3. **Run the build script**
   ```powershell
   .\build.ps1
   ```
   (or right-click the `build.ps1`-file and choose 'Run with PowerShell')
   
   The script will:
   - Validate ATAS installation
   - Download and install the latest .NET 8 SDK if needed
   - Build all projects
   - Deploy files to the correct locations

4. **Start ATAS Platform**
   - Open ATAS
   - Add the "BroadcastOrderEvents" strategy to your chart
   - Configure the strategy settings

5. **Start the Order Event Hub**
   ```powershell
   cd publish\OrderEventHub
   sadnerd.io.ATAS.OrderEventHub.exe
   ```

6. **Open the web interface**
   - Navigate to `http://localhost:15420`
   - Complete the initial setup

## Build Script Features

The `build.ps1` script includes several safety checks and features:

- **ATAS Validation**: Ensures ATAS Strategies folder exists before proceeding
- **Latest .NET SDK**: Downloads the most recent .NET 8 SDK version
- **Automatic Installation**: Installs .NET 8 SDK if not present
- **Comprehensive Validation**: Verifies each step completes successfully
- **Clear Error Messages**: Provides actionable guidance when issues occur

## Manual Installation

If you prefer to install manually or the build script fails:

### ATAS Strategy Deployment

1. **Ensure ATAS is properly installed:**
   - Verify the folder exists: `%APPDATA%\ATAS\Strategies`
   - If not, launch ATAS at least once

2. **Copy the following files to `%APPDATA%\ATAS\Strategies`:**
   - CommandLine.dll
   - log4net.dll
   - Macross.Json.Extensions.dll
   - Newtonsoft.Json.dll
   - Pipelines.Sockets.Unofficial.dll
   - protobuf-net.Core.dll
   - protobuf-net.dll
   - sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.dll
   - sadnerd.io.ATAS.BroadcastOrderEvents.dll
   - ServiceWire.dll
   - SkiaSharp.dll
   - websocket-sharp.dll

### Order Event Hub Deployment

1. **Install .NET 8 SDK:**
   - Download from [Microsoft .NET Downloads](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Install the SDK (not just the runtime)

2. **Build the project:**
   ```powershell
   dotnet publish solution\sadnerd.io.ATAS.OrderEventHub\sadnerd.io.ATAS.OrderEventHub.csproj -c Release
   ```

3. **Copy the published files to your desired location**

## Troubleshooting Installation

### Common Installation Issues

**"ATAS Strategies folder not found!"**
- **Solution:** Install ATAS Platform and launch it at least once
- The Strategies folder is created during the first launch
- Expected location: `%APPDATA%\ATAS\Strategies`

**"Failed to download .NET SDK"**
- Check your internet connection
- Temporarily disable antivirus/firewall
- Try running PowerShell as Administrator
- Manual download: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

**"Build failed" or compilation errors**
- Ensure .NET 8 SDK is properly installed: `dotnet --version`
- Try cleaning and rebuilding: `dotnet clean` then `dotnet restore`
- Check that all NuGet packages are restored

**"Access denied" when copying files**
- Run PowerShell as Administrator
- Check that ATAS is not running (it may lock strategy files)
- Verify antivirus is not blocking the operation

**"Strategy not found in ATAS"**
- Ensure all DLL files are copied to the correct ATAS Strategies folder
- Restart ATAS after copying files
- Check ATAS logs for loading errors

**"Cannot connect to Order Event Hub"**
- Verify the Order Event Hub is running
- Check firewall settings
- Ensure the correct port is configured (default: 35144 for ServiceWire, 15420 for web interface)

For additional help, see [Configuration Guide](configuration.md).
