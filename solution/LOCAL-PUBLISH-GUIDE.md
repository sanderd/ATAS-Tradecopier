# Local Publishing Guide for ATAS Trade Copy System

This guide explains how to build and publish the ATAS Trade Copy System locally using the same process as the CI pipeline.

## Files Created

1. **publish-local.ps1** - PowerShell script that replicates the CI build process
2. **publish-local.bat** - Batch file wrapper for easy execution
3. **LOCAL-PUBLISH-GUIDE.md** - This guide

## Quick Start

### Option 1: Using Batch File (Recommended)
```cmd
# Default build with "local-build" tag
publish-local.bat

# Custom tag
publish-local.bat v1.2.3-local
```

### Option 2: Using PowerShell Directly
```powershell
# Default build
.\publish-local.ps1

# Custom build with tag
.\publish-local.ps1 -Tag "v1.2.3-local"

# Debug build
.\publish-local.ps1 -Tag "debug-build" -Configuration "Debug"

# Skip tests
.\publish-local.ps1 -SkipTests
```

## What the Script Does

The script replicates the exact CI process:

1. **Sets CI environment variables** (`CI=true`)
2. **Restores dependencies** using `dotnet restore`
3. **Builds all projects** in Release configuration
4. **Runs tests** (optional, can be skipped)
5. **Publishes BroadcastOrderEvents** - the ATAS strategy DLL
6. **Publishes OrderEventHub** - the web application
7. **Creates deployment scripts**:
   - `deploy.bat` - Installs ATAS strategy
   - `start-ordereventhub.bat` - Starts web interface
8. **Generates README.md** with installation instructions
9. **Creates ZIP package** ready for distribution

## Output Structure

After running, you'll get:

```
??? publish/                          # Raw publish output
?   ??? BroadcastOrderEvents/         # ATAS strategy files
?   ??? OrderEventHub/               # Web application files
??? release-package/                  # Packaged release
?   ??? BroadcastOrderEvents/
?   ??? OrderEventHub/
?   ??? deploy.bat
?   ??? start-ordereventhub.bat
?   ??? README.md
??? atas-tradecopy-{tag}.zip         # Final package
```

## Key Features

### CI Compatibility
- Sets `CI=true` environment variable
- Uses reference assemblies when ATAS DLLs aren't available
- Skips System.* DLL inclusion in publish output
- Matches exact CI build configuration

### Error Handling
- Validates each build step
- Continues if tests fail (like CI)
- Provides clear error messages
- Cleans up on failure

### Customization
- **Tag**: Customize release version tag
- **Configuration**: Debug or Release builds
- **SkipTests**: Skip test execution for faster builds

## Installation from Local Build

1. **Extract the ZIP**: `atas-tradecopy-{tag}.zip`
2. **Deploy ATAS Strategy**: Run `deploy.bat`
   - Copies strategy files to `%APPDATA%\ATAS\Strategies\`
3. **Start Web Interface**: Run `start-ordereventhub.bat`
   - Opens http://localhost:15420 in browser
   - Starts the OrderEventHub service

## Troubleshooting

### PowerShell Execution Policy
If you get execution policy errors:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### ATAS Strategy Not Appearing
- Ensure ATAS is closed when running `deploy.bat`
- Restart ATAS after deployment
- Check `%APPDATA%\ATAS\Strategies\` for files

### Build Errors
- Ensure .NET 8 SDK is installed
- Run `dotnet --list-sdks` to verify
- Check that all NuGet packages restore correctly

### Reference Assembly Issues
The script automatically handles reference assemblies for CI builds, but if you encounter issues:
- Ensure ReferenceAssemblies projects build successfully
- Check that constructor signatures match real ATAS DLLs
- Verify CI environment variable is set correctly

## Development Workflow

For active development:

1. **Quick Test Build**:
   ```cmd
   publish-local.bat debug
   ```

2. **Full Release Build**:
   ```cmd
   publish-local.bat v1.2.3-rc1
   ```

3. **Skip Tests for Speed**:
   ```powershell
   .\publish-local.ps1 -Tag "quick-test" -SkipTests
   ```

This approach ensures your local builds match the CI environment exactly, reducing deployment issues and ensuring consistent behavior across environments.