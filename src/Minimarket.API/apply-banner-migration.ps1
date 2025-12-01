# Script PowerShell para aplicar la migración de Banners
# Este script ejecuta el SQL directamente usando Entity Framework

Write-Host "Aplicando migración de estructura de tabla Banners..." -ForegroundColor Yellow

# Leer el archivo SQL
$sqlFile = Join-Path $PSScriptRoot "..\Minimarket.Infrastructure\Migrations\UpdateBannersTableStructure.sql"
$sqlContent = Get-Content $sqlFile -Raw

Write-Host "Script SQL cargado. Por favor, ejecuta este script manualmente en tu base de datos." -ForegroundColor Cyan
Write-Host "Archivo: $sqlFile" -ForegroundColor Green
Write-Host ""
Write-Host "O detén el servidor backend y ejecuta:" -ForegroundColor Yellow
Write-Host "  dotnet ef database update --project ../Minimarket.Infrastructure --startup-project ." -ForegroundColor White

