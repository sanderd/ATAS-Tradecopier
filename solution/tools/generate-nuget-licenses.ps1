#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Generates license information for all NuGet packages in the solution.

.DESCRIPTION
    This script scans all .csproj files in the solution, extracts NuGet package references,
    and generates a comprehensive NOTICE file with license information.
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = (Get-Location),
    
    [Parameter(Mandatory = $false)]
    [string]$SolutionPath = (Get-Location)
)

# Function to get all package references from project files
function Get-AllPackageReferences {
    param([string]$SolutionPath)
    
    $packages = @{}
    $projectFiles = Get-ChildItem -Path $SolutionPath -Recurse -Filter "*.csproj"
    
    foreach ($projectFile in $projectFiles) {
        Write-Host "Processing $($projectFile.Name)..."
        
        [xml]$projectXml = Get-Content $projectFile.FullName
        $packageReferences = $projectXml.Project.ItemGroup.PackageReference
        
        if ($packageReferences) {
            foreach ($package in $packageReferences) {
                if ($package.Include -and $package.Version) {
                    $packageKey = "$($package.Include)"
                    if (-not $packages.ContainsKey($packageKey)) {
                        $packages[$packageKey] = @{
                            Name = $package.Include
                            Version = $package.Version
                        }
                    }
                }
            }
        }
    }
    
    return $packages
}

# Main execution
Write-Host "Scanning solution for NuGet packages..." -ForegroundColor Green
$packages = Get-AllPackageReferences -SolutionPath $SolutionPath

Write-Host "Found $($packages.Count) unique packages" -ForegroundColor Green

# Known license information for packages in this solution
$knownLicenses = @{
    "ServiceWire" = @{
        License = "Apache-2.0"
        LicenseUrl = "https://github.com/tylerjensen/ServiceWire/blob/master/LICENSE"
        Author = "Tyler Jensen"
    }
    "MediatR" = @{
        License = "Apache-2.0"
        LicenseUrl = "https://github.com/jbogard/MediatR/blob/master/LICENSE"
        Author = "Jimmy Bogard"
    }
    "Microsoft.AspNetCore.Identity.EntityFrameworkCore" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/aspnetcore/blob/main/LICENSE.txt"
        Author = "Microsoft Corporation"
    }
    "Microsoft.AspNetCore.Identity.UI" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/aspnetcore/blob/main/LICENSE.txt"
        Author = "Microsoft Corporation"
    }
    "Microsoft.AspNetCore.SignalR" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/aspnetcore/blob/main/LICENSE.txt"
        Author = "Microsoft Corporation"
    }
    "Microsoft.AspNetCore.SignalR.Client" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/aspnetcore/blob/main/LICENSE.txt"
        Author = "Microsoft Corporation"
    }
    "Microsoft.EntityFrameworkCore.Sqlite" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/efcore/blob/main/LICENSE.txt"
        Author = "Microsoft Corporation"
    }
    "Microsoft.EntityFrameworkCore.Tools" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/efcore/blob/main/LICENSE.txt"
        Author = "Microsoft Corporation"
    }
    "Microsoft.Extensions.Options" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"
        Author = "Microsoft Corporation"
    }
    "Microsoft.NET.Test.Sdk" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/microsoft/vstest/blob/main/LICENSE"
        Author = "Microsoft Corporation"
    }
    "Serilog.AspNetCore" = @{
        License = "Apache-2.0"
        LicenseUrl = "https://github.com/serilog/serilog-aspnetcore/blob/dev/LICENSE"
        Author = "Serilog Contributors"
    }
    "Serilog.Extensions.Hosting" = @{
        License = "Apache-2.0"
        LicenseUrl = "https://github.com/serilog/serilog-extensions-hosting/blob/dev/LICENSE"
        Author = "Serilog Contributors"
    }
    "Serilog.Sinks.Console" = @{
        License = "Apache-2.0"
        LicenseUrl = "https://github.com/serilog/serilog-sinks-console/blob/dev/LICENSE"
        Author = "Serilog Contributors"
    }
    "Serilog.Sinks.File" = @{
        License = "Apache-2.0"
        LicenseUrl = "https://github.com/serilog/serilog-sinks-file/blob/dev/LICENSE"
        Author = "Serilog Contributors"
    }
    "System.Threading.AccessControl" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/dotnet/runtime/blob/main/LICENSE.TXT"
        Author = "Microsoft Corporation"
    }
    "HttpTracer" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/BSiLabs/HttpTracer/blob/master/LICENSE"
        Author = "BSiLabs"
    }
    "RestSharp" = @{
        License = "Apache-2.0"
        LicenseUrl = "https://github.com/restsharp/RestSharp/blob/dev/LICENSE.txt"
        Author = "RestSharp Community"
    }
    "NUnit" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/nunit/nunit/blob/master/LICENSE.txt"
        Author = "NUnit Software"
    }
    "NUnit.Analyzers" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/nunit/nunit.analyzers/blob/master/LICENSE.txt"
        Author = "NUnit Software"
    }
    "NUnit3TestAdapter" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/nunit/nunit3-vs-adapter/blob/master/LICENSE.txt"
        Author = "NUnit Software"
    }
    "coverlet.collector" = @{
        License = "MIT"
        LicenseUrl = "https://github.com/coverlet-coverage/coverlet/blob/master/LICENSE"
        Author = "Coverlet Contributors"
    }
}

# Generate NOTICE content
$noticeContent = @"
NOTICE

This product includes software developed by the sadnerd.io team.

Third-Party Software Components:

The following third-party software components are distributed with this product:

"@

# Group packages by license type
$licenseGroups = @{}

foreach ($packageName in $packages.Keys) {
    $package = $packages[$packageName]
    $licenseInfo = $knownLicenses[$packageName]
    
    if ($licenseInfo) {
        $license = $licenseInfo.License
        if (-not $licenseGroups.ContainsKey($license)) {
            $licenseGroups[$license] = @()
        }
        $licenseGroups[$license] += @{
            Name = $packageName
            Version = $package.Version
            Author = $licenseInfo.Author
            LicenseUrl = $licenseInfo.LicenseUrl
        }
    } else {
        Write-Warning "License information not found for package: $packageName"
        if (-not $licenseGroups.ContainsKey("Unknown")) {
            $licenseGroups["Unknown"] = @()
        }
        $licenseGroups["Unknown"] += @{
            Name = $packageName
            Version = $package.Version
            Author = "Unknown"
            LicenseUrl = "Unknown"
        }
    }
}

# Add license sections
foreach ($license in $licenseGroups.Keys | Sort-Object) {
    $noticeContent += "`n`n$license Licensed Components:`n"
    $noticeContent += "=" * ($license.Length + 21) + "`n"
    
    foreach ($pkg in $licenseGroups[$license] | Sort-Object Name) {
        $noticeContent += "- $($pkg.Name) v$($pkg.Version) by $($pkg.Author)`n"
        if ($pkg.LicenseUrl -ne "Unknown") {
            $noticeContent += "  License: $($pkg.LicenseUrl)`n"
        }
    }
}

$noticeContent += @"


Additional Information:
======================

For complete license texts, please refer to the individual package repositories
or NuGet package metadata at https://www.nuget.org/

Microsoft packages are licensed under the MIT License.
For the complete Microsoft license text, see: https://opensource.org/licenses/MIT

Apache 2.0 license text can be found at: https://www.apache.org/licenses/LICENSE-2.0

Generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

# Write NOTICE file
$noticeFile = Join-Path $OutputPath "NOTICE"
$noticeContent | Out-File -FilePath $noticeFile -Encoding UTF8

Write-Host "NOTICE file generated successfully at: $noticeFile" -ForegroundColor Green
Write-Host "Found packages with the following license types:" -ForegroundColor Yellow
foreach ($license in $licenseGroups.Keys | Sort-Object) {
    Write-Host "  - $license ($($licenseGroups[$license].Count) packages)" -ForegroundColor Cyan
}

# Also generate a detailed CSV report
$csvFile = Join-Path $OutputPath "package-licenses.csv"
$csvContent = "Package,Version,License,Author,LicenseUrl`n"

foreach ($license in $licenseGroups.Keys) {
    foreach ($pkg in $licenseGroups[$license]) {
        $csvContent += "$($pkg.Name),$($pkg.Version),$license,$($pkg.Author),$($pkg.LicenseUrl)`n"
    }
}

$csvContent | Out-File -FilePath $csvFile -Encoding UTF8
Write-Host "Detailed CSV report generated at: $csvFile" -ForegroundColor Green