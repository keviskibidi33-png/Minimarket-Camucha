#!/bin/bash

# Script para ejecutar tests con coverage y generar reporte
# Requiere: dotnet-reportgenerator-globaltool instalado

echo "========================================="
echo "Ejecutando Tests con Code Coverage"
echo "========================================="

# Limpiar resultados anteriores
if [ -d "./TestResults" ]; then
    rm -rf ./TestResults
fi

# Ejecutar tests con coverage
echo ""
echo "[1/3] Ejecutando tests..."
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Los tests fallaron. Revisa los errores antes de generar el reporte."
    exit 1
fi

echo ""
echo "[2/3] Tests ejecutados exitosamente."

# Verificar si ReportGenerator está instalado
echo ""
echo "[3/3] Generando reporte HTML..."

if ! command -v reportgenerator &> /dev/null; then
    echo "ReportGenerator no está instalado. Instalando..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Generar reporte HTML
reportgenerator \
    -reports:"./TestResults/**/coverage.cobertura.xml" \
    -targetdir:"coveragereport" \
    -reporttypes:"Html;Badges" \
    -title:"Minimarket Backend - Code Coverage Report"

echo ""
echo "========================================="
echo "Reporte generado en: ./coveragereport/index.html"
echo "========================================="










