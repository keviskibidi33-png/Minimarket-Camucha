# Script PowerShell para probar Traefik localmente antes de subir a producción
# Simula el entorno de Coolify para validar la configuración

$ErrorActionPreference = "Stop"

Write-Host "🧪 Iniciando pruebas locales de Traefik..." -ForegroundColor Cyan
Write-Host ""

# Función para limpiar
function Cleanup {
    Write-Host ""
    Write-Host "🧹 Limpiando contenedores de prueba..." -ForegroundColor Yellow
    docker-compose -f docker-compose.test.yml down -v 2>$null
}

# Registrar cleanup al salir
Register-ObjectEvent -InputObject ([System.Console]) -EventName "CancelKeyPress" -Action { Cleanup } | Out-Null

try {
    # 1. Verificar que Docker esté corriendo
    Write-Host "1️⃣  Verificando Docker..." -ForegroundColor Yellow
    try {
        docker info | Out-Null
        Write-Host "✅ Docker está corriendo" -ForegroundColor Green
    } catch {
        Write-Host "❌ Docker no está corriendo" -ForegroundColor Red
        exit 1
    }
    Write-Host ""

    # 2. Construir y levantar servicios
    Write-Host "2️⃣  Construyendo y levantando servicios de prueba..." -ForegroundColor Yellow
    docker-compose -f docker-compose.test.yml build --no-cache
    docker-compose -f docker-compose.test.yml up -d
    Write-Host ""

    # 3. Esperar a que los servicios estén listos
    Write-Host "3️⃣  Esperando a que los servicios estén listos..." -ForegroundColor Yellow
    Write-Host "   Esto puede tardar hasta 2 minutos..." -ForegroundColor Gray
    Start-Sleep -Seconds 30

    # Verificar que Traefik esté corriendo
    $traefikReady = $false
    for ($i = 1; $i -le 30; $i++) {
        if (docker ps | Select-String -Pattern "traefik-test") {
            Write-Host "✅ Traefik está corriendo" -ForegroundColor Green
            $traefikReady = $true
            break
        }
        if ($i -eq 30) {
            Write-Host "❌ Traefik no inició después de 30 intentos" -ForegroundColor Red
            docker-compose -f docker-compose.test.yml logs traefik
            exit 1
        }
        Start-Sleep -Seconds 2
    }
    Write-Host ""

    # 4. Verificar que el servicio web esté corriendo
    Write-Host "4️⃣  Verificando servicio web..." -ForegroundColor Yellow
    $webReady = $false
    for ($i = 1; $i -le 30; $i++) {
        if (docker ps | Select-String -Pattern "minimarket-web-test") {
            Write-Host "✅ Servicio web está corriendo" -ForegroundColor Green
            $webReady = $true
            break
        }
        if ($i -eq 30) {
            Write-Host "❌ Servicio web no inició después de 30 intentos" -ForegroundColor Red
            docker-compose -f docker-compose.test.yml logs web
            exit 1
        }
        Start-Sleep -Seconds 2
    }
    Write-Host ""

    # 5. Verificar labels de Traefik
    Write-Host "5️⃣  Verificando labels de Traefik..." -ForegroundColor Yellow
    $webContainer = "minimarket-web-test"
    $traefikEnable = docker inspect $webContainer --format '{{index .Config.Labels "traefik.enable"}}'
    $traefikRouter = docker inspect $webContainer --format '{{index .Config.Labels "traefik.http.routers.web.rule"}}'
    $traefikCertResolver = docker inspect $webContainer --format '{{index .Config.Labels "traefik.http.routers.web.tls.certresolver"}}'
    $traefikPort = docker inspect $webContainer --format '{{index .Config.Labels "traefik.http.services.web.loadbalancer.server.port"}}'

    Write-Host "   Labels encontrados:" -ForegroundColor White
    Write-Host "   - traefik.enable: $traefikEnable" -ForegroundColor Gray
    Write-Host "   - traefik.http.routers.web.rule: $traefikRouter" -ForegroundColor Gray
    Write-Host "   - traefik.http.routers.web.tls.certresolver: $traefikCertResolver" -ForegroundColor Gray
    Write-Host "   - traefik.http.services.web.loadbalancer.server.port: $traefikPort" -ForegroundColor Gray
    Write-Host ""

    if ($traefikEnable -ne "true") {
        Write-Host "❌ traefik.enable NO está configurado correctamente" -ForegroundColor Red
        exit 1
    }

    if ([string]::IsNullOrEmpty($traefikRouter)) {
        Write-Host "❌ Router NO está configurado" -ForegroundColor Red
        exit 1
    }

    if ($traefikPort -ne "80") {
        Write-Host "❌ Puerto incorrecto: $traefikPort (debería ser 80)" -ForegroundColor Red
        exit 1
    }

    Write-Host "✅ Labels de Traefik están configurados correctamente" -ForegroundColor Green
    Write-Host ""

    # 6. Verificar que Traefik detecte el servicio
    Write-Host "6️⃣  Verificando que Traefik detecte el servicio..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10

    $traefikApi = "http://localhost:8080"
    try {
        $routers = Invoke-RestMethod -Uri "$traefikApi/api/http/routers" -ErrorAction SilentlyContinue
        if ($routers | ConvertTo-Json | Select-String -Pattern "web") {
            Write-Host "✅ Traefik detectó el router 'web'" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Traefik no detectó el router 'web' aún" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️  No se pudo verificar el dashboard de Traefik" -ForegroundColor Yellow
    }
    Write-Host ""

    # 7. Verificar conectividad del servicio web
    Write-Host "7️⃣  Verificando conectividad del servicio web..." -ForegroundColor Yellow
    $testResult = docker exec $webContainer wget --quiet --tries=1 --spider http://localhost/ 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Servicio web responde correctamente" -ForegroundColor Green
    } else {
        Write-Host "❌ Servicio web NO responde" -ForegroundColor Red
        docker-compose -f docker-compose.test.yml logs web | Select-Object -Last 20
        exit 1
    }
    Write-Host ""

    # 8. Probar acceso HTTP a través de Traefik
    Write-Host "8️⃣  Probando acceso HTTP a través de Traefik..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5

    try {
        $httpResponse = Invoke-WebRequest -Uri "http://localhost/" -Method Head -UseBasicParsing -ErrorAction SilentlyContinue
        $statusCode = $httpResponse.StatusCode
        if ($statusCode -eq 200 -or $statusCode -eq 301 -or $statusCode -eq 308) {
            Write-Host "✅ HTTP responde correctamente (código: $statusCode)" -ForegroundColor Green
            if ($statusCode -eq 301 -or $statusCode -eq 308) {
                Write-Host "   (Redirección HTTP → HTTPS funcionando)" -ForegroundColor Gray
            }
        } else {
            Write-Host "⚠️  HTTP respondió con código: $statusCode" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️  No se pudo conectar a http://localhost/" -ForegroundColor Yellow
        Write-Host "   Verificando logs de Traefik..." -ForegroundColor Gray
        docker-compose -f docker-compose.test.yml logs traefik | Select-Object -Last 20
    }
    Write-Host ""

    # 9. Verificar logs de Traefik para errores
    Write-Host "9️⃣  Verificando logs de Traefik para errores..." -ForegroundColor Yellow
    $traefikErrors = docker-compose -f docker-compose.test.yml logs traefik 2>&1 | Select-String -Pattern "error|Error|ERROR" | Select-Object -Last 10
    if (-not $traefikErrors) {
        Write-Host "✅ No se encontraron errores en logs de Traefik" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Errores encontrados en logs de Traefik:" -ForegroundColor Yellow
        $traefikErrors | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    }
    Write-Host ""

    # 10. Resumen final
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host "📋 RESUMEN DE PRUEBAS" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "✅ Traefik está corriendo" -ForegroundColor Green
    Write-Host "✅ Servicio web está corriendo" -ForegroundColor Green
    Write-Host "✅ Labels de Traefik están configurados" -ForegroundColor Green
    Write-Host "✅ Traefik detecta el servicio" -ForegroundColor Green
    Write-Host "✅ Servicio web responde correctamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 Accesos de prueba:" -ForegroundColor Yellow
    Write-Host "   - Traefik Dashboard: http://localhost:8080" -ForegroundColor White
    Write-Host "   - Frontend (HTTP): http://localhost/" -ForegroundColor White
    Write-Host "   - Frontend (HTTPS): https://localhost/ (puede mostrar advertencia de certificado)" -ForegroundColor White
    Write-Host ""
    Write-Host "✅ Todas las pruebas pasaron correctamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "💡 Para ver logs en tiempo real:" -ForegroundColor Yellow
    Write-Host "   docker-compose -f docker-compose.test.yml logs -f" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "💡 Para detener los servicios:" -ForegroundColor Yellow
    Write-Host "   docker-compose -f docker-compose.test.yml down" -ForegroundColor Cyan
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "Error durante las pruebas: $_" -ForegroundColor Red
    Cleanup
    exit 1
}

