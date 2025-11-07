# Code Coverage Report - Guía de Uso

## Configuración de Code Coverage

El proyecto está configurado para usar **Coverlet** para generar reportes de code coverage.

### Prerequisitos

1. **Instalar ReportGenerator** (si no está instalado):
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Ejecutar Tests con Coverage

#### Opción 1: Usar Script (Recomendado)

**Windows (PowerShell):**
```powershell
.\run-tests-with-coverage.ps1
```

**Linux/Mac (Bash):**
```bash
chmod +x run-tests-with-coverage.sh
./run-tests-with-coverage.sh
```

#### Opción 2: Comandos Manuales

**1. Ejecutar tests con coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

**2. Generar reporte HTML:**
```bash
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

**3. Abrir reporte:**
El reporte se generará en: `./coveragereport/index.html`

### Configuración de Thresholds

Los proyectos de tests tienen configurados thresholds mínimos:

- **Minimarket.UnitTests**: 80% line coverage mínimo
- **Minimarket.IntegrationTests**: Coverage tracking habilitado
- **Minimarket.FunctionalTests**: Coverage tracking habilitado

### Interpretar el Reporte

El reporte HTML muestra:

1. **Coverage por Assembly**: Coverage total por proyecto
2. **Coverage por Clase**: Coverage de cada clase
3. **Líneas Cubiertas/No Cubiertas**: Líneas específicas que no están cubiertas
4. **Branch Coverage**: Coverage de ramas condicionales

### Objetivos de Coverage

Según TASK_ASSIGNMENT_QA_Backend.md:

- **Application Layer**: >80% (CRÍTICO) ✅
- **Domain Layer**: >90% (especificaciones) ✅
- **Validators**: >90% ✅
- **Handlers**: >85% ✅

### Troubleshooting

**Problema**: ReportGenerator no encontrado
**Solución**: 
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

**Problema**: No se generan archivos .xml
**Solución**: Verificar que los tests se ejecutaron correctamente:
```bash
dotnet test
```

**Problema**: Reporte vacío
**Solución**: Verificar que los proyectos referenciados estén compilando:
```bash
dotnet build
```

### Integración con CI/CD

Para integrar en pipelines:

```yaml
# Ejemplo para Azure DevOps / GitHub Actions
- name: Run tests with coverage
  run: dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

- name: Generate coverage report
  run: reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:"Html"

- name: Publish coverage report
  uses: actions/upload-artifact@v3
  with:
    name: coverage-report
    path: coveragereport
```

### Archivos de Coverage

Los archivos generados son:

- `TestResults/**/coverage.cobertura.xml` - Datos de coverage en formato Cobertura
- `coveragereport/index.html` - Reporte HTML visual
- `coveragereport/badge_linecoverage.svg` - Badge de coverage para README

### Actualizar Coverage Regularmente

Se recomienda ejecutar coverage report:
- Antes de cada PR
- Después de agregar nuevas features
- Semanalmente para monitoreo







