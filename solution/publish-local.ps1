# Local Publish Script for ATAS Trade Copy System
# This script replicates the CI build process locally

param(
    [Parameter(Mandatory=$false)]
    [string]$Tag = "local-build",
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests = $false
)

# Set up environment variables to match CI
$env:CI = "true"
$env:DOTNET_VERSION = "8.0.x"

Write-Host "========================================" -ForegroundColor Green
Write-Host " ATAS Trade Copy System Local Build" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Tag: $Tag" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "CI Mode: $($env:CI)" -ForegroundColor Yellow
Write-Host ""

# Determine the correct solution path
$scriptLocation = $PSScriptRoot

# Check if we're already in the solution directory (contains .sln file)
if (Get-ChildItem -Name "*.sln") {
    $solutionPath = "."
    Write-Host "Found solution file in current directory: $(Get-ChildItem -Name '*.sln')" -ForegroundColor Cyan
} elseif (Test-Path "solution") {
    $solutionPath = "solution"
    Write-Host "Found solution directory at: $solutionPath" -ForegroundColor Cyan
} else {
    throw "Could not find solution directory or .sln file. Please run this script from the repository root or solution directory."
}

try {
    # Clean previous builds
    Write-Host "Cleaning previous builds..." -ForegroundColor Cyan
    if (Test-Path "publish") { Remove-Item -Recurse -Force "publish" }
    if (Test-Path "release-package") { Remove-Item -Recurse -Force "release-package" }
    
    # Navigate to solution directory if needed
    if ($solutionPath -ne ".") {
        Write-Host "Working in solution directory: $solutionPath" -ForegroundColor Cyan
        Push-Location $solutionPath
    } else {
        Write-Host "Working in current directory (solution root)" -ForegroundColor Cyan
    }
    
    # Restore dependencies
    Write-Host "Restoring dependencies..." -ForegroundColor Cyan
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    
    # Build all projects
    Write-Host "Building all projects..." -ForegroundColor Cyan
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    
    # Run tests (unless skipped)
    if (-not $SkipTests) {
        Write-Host "Running tests..." -ForegroundColor Cyan
        dotnet test --configuration $Configuration --no-build --verbosity minimal --logger trx
        # Note: Continue even if tests fail (like CI does)
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Tests failed, but continuing with publish..."
        }
    }
    
    # Return to root directory for publish if we changed directories
    if ($solutionPath -ne ".") {
        Pop-Location
    }
    
    # Create publish directory
    New-Item -ItemType Directory -Force -Path "publish" | Out-Null
    
    # Determine the relative path to projects based on where we are
    if ($solutionPath -eq ".") {
        $projectPrefix = ""
        $outputPrefix = "./publish"
    } else {
        $projectPrefix = "$solutionPath/"
        $outputPrefix = "../publish"
    }
    
    # Publish BroadcastOrderEvents
    Write-Host "Publishing BroadcastOrderEvents..." -ForegroundColor Cyan
    if ($solutionPath -ne ".") { Push-Location $solutionPath }
    dotnet publish "sadnerd.io.ATAS.BroadcastOrderEvents/sadnerd.io.ATAS.BroadcastOrderEvents.csproj" `
        --configuration $Configuration `
        --output "$outputPrefix/BroadcastOrderEvents" `
        --no-restore
    if ($LASTEXITCODE -ne 0) { throw "BroadcastOrderEvents publish failed" }
    if ($solutionPath -ne ".") { Pop-Location }
    
    # Publish OrderEventHub
    Write-Host "Publishing OrderEventHub..." -ForegroundColor Cyan
    if ($solutionPath -ne ".") { Push-Location $solutionPath }
    dotnet publish "sadnerd.io.ATAS.OrderEventHub/sadnerd.io.ATAS.OrderEventHub.csproj" `
        --configuration $Configuration `
        --output "$outputPrefix/OrderEventHub" `
        --no-restore
    if ($LASTEXITCODE -ne 0) { throw "OrderEventHub publish failed" }
    if ($solutionPath -ne ".") { Pop-Location }
    
    # Create release package
    Write-Host "Creating release package..." -ForegroundColor Cyan
    $releaseDir = "release-package"
    $packageName = "atas-tradecopy-$Tag"
    
    # Create release directory structure
    New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null
    
    # Copy BroadcastOrderEvents
    $broadcastDir = Join-Path $releaseDir "BroadcastOrderEvents"
    Copy-Item -Recurse "publish/BroadcastOrderEvents" $broadcastDir
    
    # Copy OrderEventHub  
    $hubDir = Join-Path $releaseDir "OrderEventHub"
    Copy-Item -Recurse "publish/OrderEventHub" $hubDir
    
    # Create deployment script
    $deployScript = @"
@echo off
echo ========================================
echo  ATAS Trade Copy System $Tag
echo ========================================
echo.

echo Deploying BroadcastOrderEvents to ATAS...
set "ATAS_STRATEGIES_DIR=%APPDATA%\ATAS\Strategies"

if not exist "%ATAS_STRATEGIES_DIR%" (
    echo Error: ATAS Strategies directory not found at %ATAS_STRATEGIES_DIR%
    echo Please ensure ATAS is installed and has been run at least once.
    echo.
    pause
    exit /b 1
)

echo Copying strategy files...
xcopy "BroadcastOrderEvents\*" "%ATAS_STRATEGIES_DIR%\" /s /y /q

echo.
echo ========================================
echo Strategy deployed successfully!
echo ========================================
echo.
echo The BroadcastOrderEvents strategy is now available in ATAS.
echo You can find it in the Strategies section when adding a new strategy to a chart.
echo.
pause
"@
    
    $deployScript | Out-File -FilePath "$releaseDir/deploy.bat" -Encoding ASCII
    
    # Create start script for OrderEventHub
    $startScript = @"
@echo off
echo ========================================
echo  ATAS Trade Copy System - OrderEventHub
echo ========================================
echo.

echo Starting OrderEventHub...
echo Web interface will be available at: http://localhost:15420
echo.

cd OrderEventHub
start http://localhost:15420
sadnerd.io.ATAS.OrderEventHub.exe
"@
    
    $startScript | Out-File -FilePath "$releaseDir/start-ordereventhub.bat" -Encoding ASCII
    
    # Create README
    $readme = @"
# ATAS Trade Copy System $Tag

## What's Included

- **BroadcastOrderEvents**: ATAS strategy for capturing trading events
- **OrderEventHub**: Web application for managing trade copying
- **Deployment Scripts**: Automated installation helpers

## Installation

1. Run ``deploy.bat`` to install the ATAS strategy
2. Run ``start-ordereventhub.bat`` to launch the web interface
3. Configure your trading accounts at http://localhost:15420

## Key Features

- Real-time order and position synchronization from ATAS
- Support for multiple target trading platforms
- Web-based configuration interface
- Automatic trade copying with customizable rules
- Comprehensive logging and error handling

## System Requirements

- Windows 10/11
- ATAS Platform (for strategy component)
- .NET 8.0 Runtime (included with ATAS)
- Modern web browser

## Build Information

- Version: $Tag
- Configuration: $Configuration
- Built: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
- CI Mode: $($env:CI)
"@
    
    $readme | Out-File -FilePath "$releaseDir/README.md" -Encoding UTF8
    
    # Create the release package
    $zipPath = "$packageName.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path "$releaseDir/*" -DestinationPath $zipPath -Force
    
    # Show results
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host " Build Completed Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Created release package: $zipPath" -ForegroundColor Yellow
    $size = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-Host "Package size: $size MB" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Release directory: $releaseDir" -ForegroundColor Cyan
    Write-Host "Publish directory: publish" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To deploy:" -ForegroundColor White
    Write-Host "1. Extract $zipPath" -ForegroundColor White
    Write-Host "2. Run deploy.bat to install ATAS strategy" -ForegroundColor White
    Write-Host "3. Run start-ordereventhub.bat to start web interface" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Error "Build failed: $_"
    # Make sure we're back in the original directory
    while ((Get-Location).Path -ne $scriptLocation) {
        try { Pop-Location } catch { 
            Set-Location $scriptLocation
            break 
        }
    }
    exit 1
}
finally {
    # Clean up location stack
    while ((Get-Location).Path -ne $scriptLocation) {
        try { Pop-Location } catch { break }
    }
}