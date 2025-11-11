# Script para ejecutar tests con coverage y generar reporte
# Requiere: dotnet-reportgenerator-globaltool instalado

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Ejecutando Tests con Code Coverage" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Limpiar resultados anteriores
if (Test-Path "./TestResults") {
    Remove-Item -Recurse -Force "./TestResults"
}

# Ejecutar tests con coverage
Write-Host "`n[1/3] Ejecutando tests..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Los tests fallaron. Revisa los errores antes de generar el reporte." -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/3] Tests ejecutados exitosamente." -ForegroundColor Green

# Verificar si ReportGenerator está instalado
Write-Host "`n[3/3] Generando reporte HTML..." -ForegroundColor Yellow

$reportGeneratorInstalled = Get-Command reportgenerator -ErrorAction SilentlyContinue

if (-not $reportGeneratorInstalled) {
    Write-Host "`nReportGenerator no está instalado. Instalando..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Generar reporte HTML
reportgenerator `
    -reports:"./TestResults/**/coverage.cobertura.xml" `
    -targetdir:"coveragereport" `
    -reporttypes:"Html;Badges" `
    -title:"Minimarket Backend - Code Coverage Report"

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host "Reporte generado en: ./coveragereport/index.html" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan

# Abrir reporte en navegador (opcional)
$openReport = Read-Host "`n¿Deseas abrir el reporte en el navegador? (S/N)"
if ($openReport -eq "S" -or $openReport -eq "s") {
    Start-Process "./coveragereport/index.html"
}










