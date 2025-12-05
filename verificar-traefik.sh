#!/bin/bash
# Script para verificar que Traefik esté funcionando correctamente
# Ejecutar en el servidor de producción (VPS donde está Coolify)

echo "🔍 Verificando configuración de Traefik..."
echo ""

# Colores para output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 1. Verificar que Traefik esté corriendo
echo "1️⃣  Verificando que Traefik esté corriendo..."
if docker ps | grep -q traefik; then
    echo -e "${GREEN}✅ Traefik está corriendo${NC}"
    docker ps | grep traefik
else
    echo -e "${RED}❌ Traefik NO está corriendo${NC}"
    echo "   Verifica en Coolify que Traefik esté activo"
fi
echo ""

# 2. Verificar que el servicio web esté corriendo
echo "2️⃣  Verificando que el servicio web esté corriendo..."
if docker ps | grep -q minimarket-web; then
    echo -e "${GREEN}✅ Servicio web está corriendo${NC}"
    docker ps | grep minimarket-web
else
    echo -e "${RED}❌ Servicio web NO está corriendo${NC}"
fi
echo ""

# 3. Verificar labels de Traefik en el contenedor web
echo "3️⃣  Verificando labels de Traefik en el contenedor web..."
WEB_CONTAINER=$(docker ps --format "{{.Names}}" | grep minimarket-web | head -1)
if [ -z "$WEB_CONTAINER" ]; then
    echo -e "${RED}❌ No se encontró el contenedor web${NC}"
else
    echo "   Contenedor: $WEB_CONTAINER"
    echo ""
    echo "   Labels de Traefik:"
    docker inspect $WEB_CONTAINER --format '{{range $key, $value := .Config.Labels}}{{$key}}={{$value}}{{"\n"}}{{end}}' | grep traefik
    echo ""
    
    # Verificar labels críticos
    TRAEFIK_ENABLE=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.enable"}}')
    TRAEFIK_ROUTER=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.http.routers.web.rule"}}')
    TRAEFIK_CERTRESOLVER=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.http.routers.web.tls.certresolver"}}')
    TRAEFIK_PORT=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.http.services.web.loadbalancer.server.port"}}')
    
    if [ "$TRAEFIK_ENABLE" = "true" ]; then
        echo -e "${GREEN}✅ traefik.enable=true${NC}"
    else
        echo -e "${RED}❌ traefik.enable NO está configurado${NC}"
    fi
    
    if [ -n "$TRAEFIK_ROUTER" ]; then
        echo -e "${GREEN}✅ Router configurado: $TRAEFIK_ROUTER${NC}"
    else
        echo -e "${RED}❌ Router NO está configurado${NC}"
    fi
    
    if [ "$TRAEFIK_CERTRESOLVER" = "letsencrypt" ]; then
        echo -e "${GREEN}✅ CertResolver correcto: letsencrypt${NC}"
    elif [ "$TRAEFIK_CERTRESOLVER" = "myresolver" ]; then
        echo -e "${YELLOW}⚠️  CertResolver es 'myresolver' - debería ser 'letsencrypt' para Coolify${NC}"
    else
        echo -e "${RED}❌ CertResolver NO está configurado o incorrecto: $TRAEFIK_CERTRESOLVER${NC}"
    fi
    
    if [ "$TRAEFIK_PORT" = "80" ]; then
        echo -e "${GREEN}✅ Puerto configurado correctamente: 80${NC}"
    else
        echo -e "${RED}❌ Puerto incorrecto: $TRAEFIK_PORT (debería ser 80)${NC}"
    fi
fi
echo ""

# 4. Verificar que el puerto 80 esté expuesto en el contenedor
echo "4️⃣  Verificando que el puerto 80 esté expuesto..."
if [ -n "$WEB_CONTAINER" ]; then
    EXPOSED_PORTS=$(docker inspect $WEB_CONTAINER --format '{{range $p, $conf := .NetworkSettings.Ports}}{{$p}} {{end}}')
    if echo "$EXPOSED_PORTS" | grep -q "80"; then
        echo -e "${GREEN}✅ Puerto 80 está expuesto${NC}"
    else
        echo -e "${YELLOW}⚠️  Puerto 80 NO está expuesto directamente (puede estar usando expose)${NC}"
        echo "   Verifica que coolify.yml use 'expose: - \"80\"'"
    fi
fi
echo ""

# 5. Verificar conectividad del servicio web
echo "5️⃣  Verificando conectividad del servicio web..."
if [ -n "$WEB_CONTAINER" ]; then
    if docker exec $WEB_CONTAINER wget --quiet --tries=1 --spider http://localhost/ 2>/dev/null; then
        echo -e "${GREEN}✅ Servicio web responde correctamente${NC}"
    else
        echo -e "${RED}❌ Servicio web NO responde${NC}"
        echo "   Verifica logs: docker logs $WEB_CONTAINER"
    fi
fi
echo ""

# 6. Verificar logs de Traefik para errores
echo "6️⃣  Verificando logs de Traefik (últimas 20 líneas)..."
TRAEFIK_CONTAINER=$(docker ps --format "{{.Names}}" | grep traefik | head -1)
if [ -n "$TRAEFIK_CONTAINER" ]; then
    echo "   Últimas líneas de logs:"
    docker logs --tail 20 $TRAEFIK_CONTAINER 2>&1 | grep -E "(error|Error|ERROR|warn|Warn|WARN|minimarket)" || echo "   No se encontraron errores relacionados"
else
    echo -e "${YELLOW}⚠️  No se encontró contenedor de Traefik${NC}"
fi
echo ""

# 7. Verificar DNS
echo "7️⃣  Verificando DNS..."
DOMAIN="minimarket.edvio.app"
DNS_RESULT=$(dig +short $DOMAIN)
if [ -n "$DNS_RESULT" ]; then
    echo -e "${GREEN}✅ DNS resuelve: $DOMAIN → $DNS_RESULT${NC}"
    if [ "$DNS_RESULT" = "103.138.188.233" ]; then
        echo -e "${GREEN}✅ DNS apunta al servidor correcto${NC}"
    else
        echo -e "${YELLOW}⚠️  DNS apunta a: $DNS_RESULT (esperado: 103.138.188.233)${NC}"
    fi
else
    echo -e "${RED}❌ DNS NO resuelve el dominio${NC}"
fi
echo ""

# 8. Verificar certificado SSL
echo "8️⃣  Verificando certificado SSL..."
if command -v openssl &> /dev/null; then
    CERT_INFO=$(echo | openssl s_client -connect $DOMAIN:443 -servername $DOMAIN 2>/dev/null | grep -E "Issuer|subject=" | head -2)
    if echo "$CERT_INFO" | grep -q "Let's Encrypt\|R3"; then
        echo -e "${GREEN}✅ Certificado SSL válido de Let's Encrypt${NC}"
        echo "   $CERT_INFO"
    elif echo "$CERT_INFO" | grep -q "Traefik"; then
        echo -e "${YELLOW}⚠️  Certificado por defecto de Traefik (no válido)${NC}"
        echo "   Traefik no ha generado certificado de Let's Encrypt aún"
        echo "   Verifica que certresolver=letsencrypt esté configurado"
    else
        echo -e "${RED}❌ No se pudo verificar el certificado${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  openssl no está instalado, no se puede verificar certificado${NC}"
fi
echo ""

# 9. Resumen y recomendaciones
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📋 RESUMEN Y RECOMENDACIONES"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "Para verificar manualmente:"
echo "1. Accede a Traefik Dashboard (si está habilitado):"
echo "   http://tu-servidor:8080 (o el puerto configurado)"
echo ""
echo "2. Verifica routers de Traefik:"
echo "   curl http://localhost:8080/api/http/routers 2>/dev/null | grep minimarket"
echo ""
echo "3. Verifica servicios:"
echo "   curl http://localhost:8080/api/http/services 2>/dev/null | grep minimarket"
echo ""
echo "4. Prueba el dominio:"
echo "   curl -I https://minimarket.edvio.app"
echo "   Debe devolver: HTTP/2 200"
echo ""
echo "5. Verifica logs completos:"
echo "   docker logs $TRAEFIK_CONTAINER"
echo ""

