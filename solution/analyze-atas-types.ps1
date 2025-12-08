# Enhanced script to extract ATAS type information for creating proper reference assemblies
param(
    [string]$AtasPath = "C:\Program Files (x86)\ATAS Platform"
)

function Write-ColorOutput {
    param([string]$Text, [string]$Color = "White")
    Write-Host $Text -ForegroundColor $Color
}

function Analyze-Assembly {
    param([string]$AssemblyPath, [string[]]$TypesOfInterest)
    
    try {
        Write-ColorOutput "Analyzing: $AssemblyPath" "Yellow"
        
        # Use LoadFrom to handle dependencies better
        $assembly = [System.Reflection.Assembly]::LoadFrom($AssemblyPath)
        
        foreach ($typeName in $TypesOfInterest) {
            $type = $assembly.GetType($typeName)
            if ($type) {
                Write-ColorOutput "`n=== $($type.FullName) ===" "Green"
                Write-ColorOutput "Assembly: $($type.Assembly.GetName().Name)"
                Write-ColorOutput "Is Interface: $($type.IsInterface)"
                Write-ColorOutput "Is Abstract: $($type.IsAbstract)"
                Write-ColorOutput "Is Class: $($type.IsClass)"
                
                if ($type.BaseType) {
                    Write-ColorOutput "Base Type: $($type.BaseType.FullName)"
                }
                
                $interfaces = $type.GetInterfaces()
                if ($interfaces.Length -gt 0) {
                    Write-ColorOutput "Interfaces: $($interfaces.Name -join ', ')"
                }
                
                Write-ColorOutput "`nPublic Properties:" "Cyan"
                $type.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) | ForEach-Object {
                    $getter = if ($_.GetGetMethod()) { "get;" } else { "" }
                    $setter = if ($_.GetSetMethod()) { "set;" } else { "" }
                    Write-ColorOutput "  $($_.PropertyType.Name) $($_.Name) { $getter $setter }"
                }
                
                Write-ColorOutput "`nProtected Properties:" "Cyan"
                $type.GetProperties([System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.GetGetMethod($true) -and $_.GetGetMethod($true).IsFamily } | ForEach-Object {
                    $getter = if ($_.GetGetMethod($true)) { "get;" } else { "" }
                    $setter = if ($_.GetSetMethod($true)) { "set;" } else { "" }
                    Write-ColorOutput "  protected $($_.PropertyType.Name) $($_.Name) { $getter $setter }"
                }
                
                Write-ColorOutput "`nPublic Methods:" "Cyan"
                $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { !$_.IsSpecialName -and $_.DeclaringType -eq $type } | ForEach-Object {
                    $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                    Write-ColorOutput "  $($_.ReturnType.Name) $($_.Name)($params)"
                }
                
                Write-ColorOutput "`nProtected Virtual Methods:" "Cyan"
                $type.GetMethods([System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance) | Where-Object { $_.IsVirtual -and $_.IsFamily -and $_.DeclaringType -eq $type } | ForEach-Object {
                    $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
                    Write-ColorOutput "  protected virtual $($_.ReturnType.Name) $($_.Name)($params)"
                }
            } else {
                Write-ColorOutput "Type not found: $typeName" "Red"
            }
        }
    } catch {
        Write-ColorOutput "Error analyzing $AssemblyPath`: $_" "Red"
    }
}

try {
    Write-ColorOutput "Enhanced ATAS Assembly Analysis" "Green"
    Write-ColorOutput "================================" "Green"
    
    # Load assemblies and analyze key types
    $assemblies = @(
        @{
            Path = "$AtasPath\ATAS.DataFeedsCore.dll"
            Types = @(
                "ATAS.DataFeedsCore.Order",
                "ATAS.DataFeedsCore.Position", 
                "ATAS.DataFeedsCore.Portfolio",
                "ATAS.DataFeedsCore.Security",
                "ATAS.DataFeedsCore.TradingManager"
            )
        },
        @{
            Path = "$AtasPath\ATAS.Indicators.dll"
            Types = @(
                "ATAS.Indicators.Indicator",
                "ATAS.Indicators.ITradingManager"
            )
        },
        @{
            Path = "$AtasPath\ATAS.Strategies.dll"
            Types = @(
                "ATAS.Strategies.Chart.ChartStrategy"
            )
        }
    )
    
    foreach ($assembly in $assemblies) {
        if (Test-Path $assembly.Path) {
            Analyze-Assembly -AssemblyPath $assembly.Path -TypesOfInterest $assembly.Types
            Write-ColorOutput "`n" + "="*80 + "`n"
        } else {
            Write-ColorOutput "Assembly not found: $($assembly.Path)" "Red"
        }
    }
    
    # Also search for any types containing "Trading" in their name
    Write-ColorOutput "Searching for Trading-related types..." "Yellow"
    
    Get-ChildItem "$AtasPath\*.dll" | ForEach-Object {
        try {
            $assembly = [System.Reflection.Assembly]::LoadFrom($_.FullName)
            $assembly.GetTypes() | Where-Object { $_.Name -like "*Trading*" -or $_.Name -like "*Manager*" } | ForEach-Object {
                Write-ColorOutput "Found: $($_.FullName) in $($_.Assembly.GetName().Name)" "Magenta"
            }
        } catch {
            # Ignore assemblies that can't be loaded
        }
    }
    
    Write-ColorOutput "`nAnalysis Complete!" "Green"
    
} catch {
    Write-ColorOutput "Failed to analyze ATAS assemblies: $_" "Red"
    Write-Host "Make sure ATAS is installed and the path is correct: $AtasPath" -ForegroundColor Yellow
}