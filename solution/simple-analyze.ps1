try {
    $indicatorsAssembly = [System.Reflection.Assembly]::LoadFrom('C:\Program Files (x86)\ATAS Platform\ATAS.Indicators.dll')
    Write-Host 'Loaded ATAS.Indicators successfully' -ForegroundColor Green
    
    $tradingTypes = $indicatorsAssembly.GetTypes() | Where-Object { $_.Name -like '*Trading*' }
    Write-Host 'Found Trading-related types:' -ForegroundColor Yellow
    $tradingTypes | ForEach-Object { Write-Host "  $($_.FullName)" }
    
    $itradingManager = $indicatorsAssembly.GetType('ATAS.Indicators.ITradingManager')
    if ($itradingManager) {
        Write-Host 'Found ITradingManager interface:' -ForegroundColor Green
        Write-Host "Properties:" -ForegroundColor Cyan
        $itradingManager.GetProperties() | ForEach-Object {
            Write-Host "  $($_.PropertyType.Name) $($_.Name)"
        }
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}