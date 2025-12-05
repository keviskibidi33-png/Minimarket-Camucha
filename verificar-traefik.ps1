# Script PowerShell para verificar que Traefik esté funcionando correctamente
# Ejecutar en el servidor de producción (VPS donde está Coolify)

Write-Host "🔍 Verificando configuración de Traefik..." -ForegroundColor Cyan
Write-Host ""

# 1. Verificar que Traefik esté corriendo
Write-Host "1️⃣  Verificando que Traefik esté corriendo..." -ForegroundColor Yellow
$traefikContainer = docker ps --format "{{.Names}}" | Select-String -Pattern "traefik"
if ($traefikContainer) {
    Write-Host "✅ Traefik está corriendo" -ForegroundColor Green
    docker ps | Select-String -Pattern "traefik"
} else {
    Write-Host "❌ Traefik NO está corriendo" -ForegroundColor Red
    Write-Host "   Verifica en Coolify que Traefik esté activo" -ForegroundColor Yellow
}
Write-Host ""

# 2. Verificar que el servicio web esté corriendo
Write-Host "2️⃣  Verificando que el servicio web esté corriendo..." -ForegroundColor Yellow
$webContainer = docker ps --format "{{.Names}}" | Select-String -Pattern "minimarket-web|web-"
if ($webContainer) {
    Write-Host "✅ Servicio web está corriendo" -ForegroundColor Green
    docker ps | Select-String -Pattern "minimarket-web|web-"
} else {
    Write-Host "❌ Servicio web NO está corriendo" -ForegroundColor Red
}
Write-Host ""

# 3. Verificar labels de Traefik en el contenedor web
Write-Host "3️⃣  Verificando labels de Traefik en el contenedor web..." -ForegroundColor Yellow
if ($webContainer) {
    $containerName = $webContainer.ToString().Trim()
    Write-Host "   Contenedor: $containerName" -ForegroundColor White
    Write-Host ""
    Write-Host "   Labels de Traefik:" -ForegroundColor White
    
    $labels = docker inspect $containerName --format '{{range $key, $value := .Config.Labels}}{{$key}}={{$value}}{{"\n"}}{{end}}' | Select-String -Pattern "traefik"
    $labels | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    Write-Host ""
    
    # Verificar labels críticos
    $traefikEnable = docker inspect $containerName --format '{{index .Config.Labels "traefik.enable"}}'
    $traefikRouter = docker inspect $containerName --format '{{index .Config.Labels "traefik.http.routers.web.rule"}}'
    $traefikCertResolver = docker inspect $containerName --format '{{index .Config.Labels "traefik.http.routers.web.tls.certresolver"}}'
    $traefikPort = docker inspect $containerName --format '{{index .Config.Labels "traefik.http.services.web.loadbalancer.server.port"}}'
    
    if ($traefikEnable -eq "true") {
        Write-Host "✅ traefik.enable=true" -ForegroundColor Green
    } else {
        Write-Host "❌ traefik.enable NO está configurado" -ForegroundColor Red
    }
    
    if ($traefikRouter) {
        Write-Host "✅ Router configurado: $traefikRouter" -ForegroundColor Green
    } else {
        Write-Host "❌ Router NO está configurado" -ForegroundColor Red
    }
    
    if ($traefikCertResolver -eq "letsencrypt") {
        Write-Host "✅ CertResolver correcto: letsencrypt" -ForegroundColor Green
    } elseif ($traefikCertResolver -eq "myresolver") {
        Write-Host "⚠️  CertResolver es 'myresolver' - debería ser 'letsencrypt' para Coolify" -ForegroundColor Yellow
    } else {
        Write-Host "❌ CertResolver NO está configurado o incorrecto: $traefikCertResolver" -ForegroundColor Red
    }
    
    if ($traefikPort -eq "80") {
        Write-Host "✅ Puerto configurado correctamente: 80" -ForegroundColor Green
    } else {
        Write-Host "❌ Puerto incorrecto: $traefikPort (debería ser 80)" -ForegroundColor Red
    }
} else {
    Write-Host "❌ No se encontró el contenedor web" -ForegroundColor Red
}
Write-Host ""

# 4. Verificar conectividad del servicio web
Write-Host "4️⃣  Verificando conectividad del servicio web..." -ForegroundColor Yellow
if ($webContainer) {
    $containerName = $webContainer.ToString().Trim()
    $testResult = docker exec $containerName wget --quiet --tries=1 --spider http://localhost/ 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Servicio web responde correctamente" -ForegroundColor Green
    } else {
        Write-Host "❌ Servicio web NO responde" -ForegroundColor Red
        Write-Host "   Verifica logs: docker logs $containerName" -ForegroundColor Yellow
    }
}
Write-Host ""

# 5. Verificar logs de Traefik
Write-Host "5️⃣  Verificando logs de Traefik (últimas 20 líneas)..." -ForegroundColor Yellow
if ($traefikContainer) {
    $traefikName = $traefikContainer.ToString().Trim()
    Write-Host "   Últimas líneas de logs:" -ForegroundColor White
    docker logs --tail 20 $traefikName 2>&1 | Select-String -Pattern "error|Error|ERROR|warn|Warn|WARN|minimarket" | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    if (-not $?) {
        Write-Host "   No se encontraron errores relacionados" -ForegroundColor Gray
    }
} else {
    Write-Host "⚠️  No se encontró contenedor de Traefik" -ForegroundColor Yellow
}
Write-Host ""

# 6. Verificar DNS
Write-Host "6️⃣  Verificando DNS..." -ForegroundColor Yellow
$domain = "minimarket.edvio.app"
try {
    $dnsResult = Resolve-DnsName -Name $domain -Type A -ErrorAction Stop | Select-Object -First 1
    if ($dnsResult) {
        $ip = $dnsResult.IPAddress
        Write-Host "✅ DNS resuelve: $domain → $ip" -ForegroundColor Green
        if ($ip -eq "103.138.188.233") {
            Write-Host "✅ DNS apunta al servidor correcto" -ForegroundColor Green
        } else {
            Write-Host "⚠️  DNS apunta a: $ip (esperado: 103.138.188.233)" -ForegroundColor Yellow
        }
    }
} catch {
    Write-Host "❌ DNS NO resuelve el dominio" -ForegroundColor Red
}
Write-Host ""

# 7. Verificar certificado SSL
Write-Host "7️⃣  Verificando certificado SSL..." -ForegroundColor Yellow
try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient($domain, 443)
    $sslStream = New-Object System.Net.Security.SslStream($tcpClient.GetStream())
    $sslStream.AuthenticateAsClient($domain)
    $cert = $sslStream.RemoteCertificate
    $cert2 = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($cert)
    
    $issuer = $cert2.Issuer
    if ($issuer -match "Let's Encrypt|R3") {
        Write-Host "✅ Certificado SSL válido de Let's Encrypt" -ForegroundColor Green
        Write-Host "   Issuer: $issuer" -ForegroundColor Gray
    } elseif ($issuer -match "Traefik") {
        Write-Host "⚠️  Certificado por defecto de Traefik (no válido)" -ForegroundColor Yellow
        Write-Host "   Traefik no ha generado certificado de Let's Encrypt aún" -ForegroundColor Yellow
        Write-Host "   Verifica que certresolver=letsencrypt esté configurado" -ForegroundColor Yellow
    } else {
        Write-Host "⚠️  Certificado: $issuer" -ForegroundColor Yellow
    }
    $sslStream.Close()
    $tcpClient.Close()
} catch {
    Write-Host "⚠️  No se pudo verificar el certificado: $_" -ForegroundColor Yellow
}
Write-Host ""

# 8. Resumen y recomendaciones
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📋 RESUMEN Y RECOMENDACIONES" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para verificar manualmente:" -ForegroundColor White
Write-Host "1. Prueba el dominio:" -ForegroundColor Yellow
Write-Host "   curl -I https://minimarket.edvio.app" -ForegroundColor Cyan
Write-Host "   Debe devolver: HTTP/2 200" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Verifica logs completos de Traefik:" -ForegroundColor Yellow
if ($traefikContainer) {
    Write-Host "   docker logs $($traefikContainer.ToString().Trim())" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "3. Verifica logs del servicio web:" -ForegroundColor Yellow
if ($webContainer) {
    Write-Host "   docker logs $($webContainer.ToString().Trim())" -ForegroundColor Cyan
}
Write-Host ""

