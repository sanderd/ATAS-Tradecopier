#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Build and deploy ATAS Trade Copy System
.DESCRIPTION
    This script installs .NET 8 SDK if required, builds the solution, and deploys components to their target locations.
#>

param(
    [switch]$SkipDotNetCheck,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Set verbose preference if requested
if ($Verbose) {
    $VerbosePreference = "Continue"
}

Write-Host "?? ATAS Trade Copy System - Build Script" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

function pause($message)
{
    # Check if running Powershell ISE
    if ($psISE)
    {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show("$message")
    }
    else
    {
        Write-Host "$message" -ForegroundColor Yellow
        $x = $host.ui.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
}

# Function to get the latest .NET 8 SDK download URL
function Get-LatestDotNet8SdkUrl {
    try {
        Write-Verbose "Fetching latest .NET 8 SDK release information..."
        
        # Use the GitHub API for dotnet/installer releases
        $githubApiUrl = "https://api.github.com/repos/dotnet/installer/releases"
        
        try {
            $releases = Invoke-RestMethod -Uri $githubApiUrl -UseBasicParsing -Headers @{
                "User-Agent" = "ATAS-TradeCopy-BuildScript/1.0"
                "Accept" = "application/vnd.github.v3+json"
            }
            
            # Filter for .NET 8.0 SDK releases
            $net8Releases = $releases | Where-Object { 
                $_.tag_name -match "^v8\.0\.\d+(-\w+\.\d+)?$" -and 
                -not $_.prerelease -and
                $_.tag_name -notmatch "(preview|rc|alpha|beta)"
            } | Sort-Object { 
                # Parse version for proper sorting
                $versionPart = $_.tag_name -replace '^v', '' -replace '-.*$', ''
                [System.Version]$versionPart 
            } -Descending
            
            if ($net8Releases.Count -eq 0) {
                throw "No stable .NET 8.0 releases found in GitHub"
            }
            
            $latestRelease = $net8Releases[0]
            $version = $latestRelease.tag_name -replace '^v', ''
            
            Write-Verbose "Latest .NET 8 SDK version found: $version"
            
            # Use the correct builds.dotnet.microsoft.com URL pattern for newer versions
            $downloadUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/$version/dotnet-sdk-$version-win-x64.exe"
            
            # Verify the URL exists before returning it
            try {
                $headRequest = Invoke-WebRequest -Uri $downloadUrl -Method Head -UseBasicParsing -TimeoutSec 15
                if ($headRequest.StatusCode -eq 200) {
                    Write-Verbose "Download URL verified: $downloadUrl"
                    return @{
                        Version = $version
                        Url = $downloadUrl
                    }
                }
            }
            catch {
                Write-Verbose "builds.dotnet.microsoft.com URL not accessible, trying alternative patterns..."
                
                # Try the older download.microsoft.com pattern for fallback
                $alternativeUrl = "https://download.microsoft.com/download/dotnet/Sdk/$version/dotnet-sdk-$version-win-x64.exe"
                try {
                    $headRequest = Invoke-WebRequest -Uri $alternativeUrl -Method Head -UseBasicParsing -TimeoutSec 15
                    if ($headRequest.StatusCode -eq 200) {
                        Write-Verbose "Alternative download URL verified: $alternativeUrl"
                        return @{
                            Version = $version
                            Url = $alternativeUrl
                        }
                    }
                }
                catch {
                    Write-Verbose "Alternative URL also failed, will use builds.dotnet.microsoft.com anyway"
                }
                
                # Return the builds.dotnet.microsoft.com URL even if verification failed
                # The download attempt will reveal if it's actually available
                return @{
                    Version = $version
                    Url = $downloadUrl
                }
            }
        }
        catch {
            Write-Warning "GitHub API failed: $($_.Exception.Message)"
            throw
        }
    }
    catch {
        Write-Warning "Failed to get latest .NET 8 version dynamically: $($_.Exception.Message)"
        Write-Verbose "Using known stable version as fallback"
        
        # Use a recent known stable version as fallback
        $knownVersion = "8.0.414"
        
        # Try both URL patterns for the fallback version
        $primaryFallbackUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/$knownVersion/dotnet-sdk-$knownVersion-win-x64.exe"
        $secondaryFallbackUrl = "https://download.microsoft.com/download/8/4/8/848f28ae-78c0-4b6d-ac2d-348cbc8b9824/dotnet-sdk-$knownVersion-win-x64.exe"
        
        # Test which fallback URL works
        foreach ($testUrl in @($primaryFallbackUrl, $secondaryFallbackUrl)) {
            try {
                $headRequest = Invoke-WebRequest -Uri $testUrl -Method Head -UseBasicParsing -TimeoutSec 10
                if ($headRequest.StatusCode -eq 200) {
                    Write-Verbose "Fallback version $knownVersion confirmed available at: $testUrl"
                    return @{
                        Version = $knownVersion
                        Url = $testUrl
                    }
                }
            }
            catch {
                Write-Verbose "Fallback URL failed: $testUrl"
                continue
            }
        }
        
        # If both fallback URLs fail, return the primary one anyway
        Write-Warning "Could not verify any fallback URL, using primary fallback"
        return @{
            Version = $knownVersion
            Url = $primaryFallbackUrl
        }
    }
}

# Function to check if .NET 8 SDK is installed
function Test-DotNet8SdkInstalled {
    try {
        $dotnetInfo = & dotnet --list-sdks 2>$null
        $hasNet8Sdk = $dotnetInfo | Where-Object { $_ -match "8\.\d+\.\d+" }
        return $null -ne $hasNet8Sdk
    }
    catch {
        return $false
    }
}

# Function to install .NET 8 SDK
function Install-DotNet8Sdk {
    Write-Host "?? Installing latest .NET 8 SDK..." -ForegroundColor Yellow
    
    $sdkInfo = Get-LatestDotNet8SdkUrl
    $installerPath = "$env:TEMP\dotnet-sdk-$($sdkInfo.Version)-win-x64.exe"
    
    try {
        Write-Host "   Downloading .NET 8 SDK version $($sdkInfo.Version)..." -ForegroundColor Gray
        Write-Verbose "   Download URL: $($sdkInfo.Url)"
        
        # Download with progress indication and better error handling
        $webClient = New-Object System.Net.WebClient
        
        # Set up progress reporting
        Register-ObjectEvent -InputObject $webClient -EventName DownloadProgressChanged -SourceIdentifier WebClientProgress -Action {
            $percent = $Event.SourceEventArgs.ProgressPercentage
            if ($percent -gt 0 -and $percent % 10 -eq 0) {  # Show progress every 10%
                Write-Host "   Download progress: $percent%" -ForegroundColor Gray
            }
        } | Out-Null
        
        try {
            $webClient.DownloadFile($sdkInfo.Url, $installerPath)
        }
        finally {
            Unregister-Event -SourceIdentifier WebClientProgress -ErrorAction SilentlyContinue
            $webClient.Dispose()
        }
        
        # Verify the download
        if (-not (Test-Path $installerPath)) {
            throw "Downloaded installer file not found at $installerPath"
        }
        
        $fileInfo = Get-Item $installerPath
        if ($fileInfo.Length -lt 1MB) {
            throw "Downloaded file appears to be incomplete (size: $($fileInfo.Length) bytes)"
        }
        
        Write-Host "   Installing .NET 8 SDK (this may take a few minutes)..." -ForegroundColor Gray
        $installProcess = Start-Process -FilePath $installerPath -ArgumentList "/quiet", "/norestart" -Wait -PassThru
        
        if ($installProcess.ExitCode -ne 0) {
            throw "SDK installation failed with exit code: $($installProcess.ExitCode)"
        }
        
        # Clean up installer
        Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
        
        Write-Host "? .NET 8 SDK installed successfully" -ForegroundColor Green
        
        # Give the system a moment to register the installation
        Start-Sleep -Seconds 2
        
    }
    catch {
        # Clean up on failure
        if (Test-Path $installerPath) {
            Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
        }
        
        Write-Host ""
        Write-Host "? Failed to install .NET 8 SDK automatically" -ForegroundColor Red
        Write-Host ""
        Write-Host "Error details: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Please install .NET 8 SDK manually:" -ForegroundColor Cyan
        Write-Host "1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Gray
        Write-Host "2. Download the SDK (not just runtime) for Windows x64" -ForegroundColor Gray
        Write-Host "3. Run the installer as Administrator" -ForegroundColor Gray
        Write-Host "4. Re-run this build script" -ForegroundColor Gray
        Write-Host ""

        pause("Press any key to exit")
        exit 1
    }
}

# Function to validate ATAS installation
function Test-AtasInstallation {
    $atasStrategiesDir = Join-Path $env:APPDATA "ATAS\Strategies"
    
    if (-not (Test-Path $atasStrategiesDir)) {
        Write-Host ""
        Write-Host "? ATAS Strategies folder not found!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Expected location: $atasStrategiesDir" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Please ensure ATAS Platform is installed and has been run at least once." -ForegroundColor Yellow
        Write-Host "The Strategies folder is created when ATAS is first launched." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To resolve this issue:" -ForegroundColor Cyan
        Write-Host "1. Install ATAS Platform from https://atas.net" -ForegroundColor Gray
        Write-Host "2. Launch ATAS at least once to create the user folders" -ForegroundColor Gray
        Write-Host "3. Re-run this build script" -ForegroundColor Gray
        Write-Host ""
        
        return $false
    }
    
    Write-Host "? ATAS Strategies folder found: $atasStrategiesDir" -ForegroundColor Green
    return $true
}

# Check ATAS installation first
Write-Host "?? Validating ATAS installation..." -ForegroundColor Blue
if (-not (Test-AtasInstallation)) {
    pause("Press any key to exit")
    exit 1
}

# Check .NET 8 SDK installation
if (-not $SkipDotNetCheck) {
    Write-Host "?? Checking .NET 8 SDK installation..." -ForegroundColor Blue
    
    if (-not (Test-DotNet8SdkInstalled)) {
        Write-Host "? .NET 8 SDK not found" -ForegroundColor Red
        Install-DotNet8Sdk
        
        # Verify installation
        if (-not (Test-DotNet8SdkInstalled)) {
            Write-Error "Failed to verify .NET 8 SDK installation. Please install manually from https://dotnet.microsoft.com/download/dotnet/8.0"
            pause("Press any key to exit")
            exit 1
        }
    }
    else {
        Write-Host "? .NET 8 SDK is installed" -ForegroundColor Green
    }
}

# Navigate to solution directory
$solutionDir = Join-Path $PSScriptRoot "solution"
if (-not (Test-Path $solutionDir)) {
    Write-Error "Solution directory not found: $solutionDir"
    pause("Press any key to exit")
    exit 1
}

Set-Location $solutionDir

Write-Host "??? Building solution..." -ForegroundColor Blue

# Restore packages
Write-Verbose "Restoring NuGet packages..."
& dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to restore packages"
    pause("Press any key to exit")
    exit 1
}

# Build BroadcastOrderEvents project
Write-Host "?? Publishing ATAS.BroadcastOrderEvents..." -ForegroundColor Blue
$publishDir = Join-Path $PSScriptRoot "publish\BroadcastOrderEvents"
& dotnet publish "sadnerd.io.ATAS.BroadcastOrderEvents\sadnerd.io.ATAS.BroadcastOrderEvents.csproj" --configuration Release --output $publishDir --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build BroadcastOrderEvents project"
    pause("Press any key to exit")
    exit 1
}

# Publish OrderEventHub project
Write-Host "?? Publishing OrderEventHub..." -ForegroundColor Blue
$publishDir = Join-Path $PSScriptRoot "publish\OrderEventHub"
& dotnet publish "sadnerd.io.ATAS.OrderEventHub\sadnerd.io.ATAS.OrderEventHub.csproj" --configuration Release --output $publishDir --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish OrderEventHub project"
    pause("Press any key to exit")
    exit 1
}

# Deploy ATAS Strategy files
Write-Host "?? Deploying ATAS Strategy..." -ForegroundColor Blue

$atasStrategiesDir = Join-Path $env:APPDATA "ATAS\Strategies"

# Files to deploy to ATAS Strategies folder
$filesToDeploy = @(
    "CommandLine.dll",
    "log4net.dll",
    "Macross.Json.Extensions.dll",
    "Newtonsoft.Json.dll",
    "Pipelines.Sockets.Unofficial.dll",
    "protobuf-net.Core.dll",
    "protobuf-net.dll",
    "sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.dll",
    "sadnerd.io.ATAS.BroadcastOrderEvents.dll",
    "ServiceWire.dll",
    "SkiaSharp.dll",
    "websocket-sharp.dll"
)

$sourceDir = Join-Path $PSScriptRoot "publish\BroadcastOrderEvents"

foreach ($file in $filesToDeploy) {
    $sourcePath = Join-Path $sourceDir $file
    $destPath = Join-Path $atasStrategiesDir $file
    
    if (Test-Path $sourcePath) {
        Write-Verbose "Copying $file to ATAS Strategies folder..."
        Copy-Item $sourcePath $destPath -Force
    }
    else {
        Write-Warning "File not found: $sourcePath"
    }
}

Write-Host ""
Write-Host "?? Build and deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Deployment locations:" -ForegroundColor Cyan
Write-Host "   ATAS Strategy: $atasStrategiesDir" -ForegroundColor Gray
Write-Host "   OrderEventHub: $publishDir" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Next steps:" -ForegroundColor Cyan
Write-Host "   1. Start ATAS and add the BroadcastOrderEvents strategy" -ForegroundColor Gray
Write-Host "   2. Run OrderEventHub: $publishDir\sadnerd.io.ATAS.OrderEventHub.exe" -ForegroundColor Gray
Write-Host "   3. Configure your trading setup via the web interface" -ForegroundColor Gray

pause("Press any key to exit")