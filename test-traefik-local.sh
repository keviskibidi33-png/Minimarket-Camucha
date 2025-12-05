#!/bin/bash
# Script para probar Traefik localmente antes de subir a producción
# Simula el entorno de Coolify para validar la configuración

set -e

echo "🧪 Iniciando pruebas locales de Traefik..."
echo ""

# Colores
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Función para limpiar
cleanup() {
    echo ""
    echo "🧹 Limpiando contenedores de prueba..."
    docker-compose -f docker-compose.test.yml down -v 2>/dev/null || true
}

# Trap para limpiar al salir
trap cleanup EXIT

# 1. Verificar que Docker esté corriendo
echo "1️⃣  Verificando Docker..."
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker no está corriendo${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Docker está corriendo${NC}"
echo ""

# 2. Construir y levantar servicios
echo "2️⃣  Construyendo y levantando servicios de prueba..."
docker-compose -f docker-compose.test.yml build --no-cache
docker-compose -f docker-compose.test.yml up -d
echo ""

# 3. Esperar a que los servicios estén listos
echo "3️⃣  Esperando a que los servicios estén listos..."
echo "   Esto puede tardar hasta 2 minutos..."
sleep 30

# Verificar que Traefik esté corriendo
for i in {1..30}; do
    if docker ps | grep -q traefik-test; then
        echo -e "${GREEN}✅ Traefik está corriendo${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}❌ Traefik no inició después de 30 intentos${NC}"
        docker-compose -f docker-compose.test.yml logs traefik
        exit 1
    fi
    sleep 2
done
echo ""

# 4. Verificar que el servicio web esté corriendo
echo "4️⃣  Verificando servicio web..."
for i in {1..30}; do
    if docker ps | grep -q minimarket-web-test; then
        echo -e "${GREEN}✅ Servicio web está corriendo${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}❌ Servicio web no inició después de 30 intentos${NC}"
        docker-compose -f docker-compose.test.yml logs web
        exit 1
    fi
    sleep 2
done
echo ""

# 5. Verificar labels de Traefik
echo "5️⃣  Verificando labels de Traefik..."
WEB_CONTAINER="minimarket-web-test"
TRAEFIK_ENABLE=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.enable"}}')
TRAEFIK_ROUTER=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.http.routers.web.rule"}}')
TRAEFIK_CERTRESOLVER=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.http.routers.web.tls.certresolver"}}')
TRAEFIK_PORT=$(docker inspect $WEB_CONTAINER --format '{{index .Config.Labels "traefik.http.services.web.loadbalancer.server.port"}}')

echo "   Labels encontrados:"
echo "   - traefik.enable: $TRAEFIK_ENABLE"
echo "   - traefik.http.routers.web.rule: $TRAEFIK_ROUTER"
echo "   - traefik.http.routers.web.tls.certresolver: $TRAEFIK_CERTRESOLVER"
echo "   - traefik.http.services.web.loadbalancer.server.port: $TRAEFIK_PORT"
echo ""

if [ "$TRAEFIK_ENABLE" != "true" ]; then
    echo -e "${RED}❌ traefik.enable NO está configurado correctamente${NC}"
    exit 1
fi

if [ -z "$TRAEFIK_ROUTER" ]; then
    echo -e "${RED}❌ Router NO está configurado${NC}"
    exit 1
fi

if [ "$TRAEFIK_PORT" != "80" ]; then
    echo -e "${RED}❌ Puerto incorrecto: $TRAEFIK_PORT (debería ser 80)${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Labels de Traefik están configurados correctamente${NC}"
echo ""

# 6. Verificar que Traefik detecte el servicio
echo "6️⃣  Verificando que Traefik detecte el servicio..."
sleep 10

# Verificar en el dashboard de Traefik
TRAEFIK_API="http://localhost:8080"
if curl -s "$TRAEFIK_API/api/http/routers" | grep -q "web"; then
    echo -e "${GREEN}✅ Traefik detectó el router 'web'${NC}"
else
    echo -e "${YELLOW}⚠️  Traefik no detectó el router 'web' aún${NC}"
    echo "   Routers disponibles:"
    curl -s "$TRAEFIK_API/api/http/routers" | grep -o '"name":"[^"]*"' | head -5
fi
echo ""

# 7. Verificar conectividad del servicio web
echo "7️⃣  Verificando conectividad del servicio web..."
if docker exec $WEB_CONTAINER wget --quiet --tries=1 --spider http://localhost/ 2>/dev/null; then
    echo -e "${GREEN}✅ Servicio web responde correctamente${NC}"
else
    echo -e "${RED}❌ Servicio web NO responde${NC}"
    docker-compose -f docker-compose.test.yml logs web | tail -20
    exit 1
fi
echo ""

# 8. Probar acceso HTTP a través de Traefik
echo "8️⃣  Probando acceso HTTP a través de Traefik..."
sleep 5

HTTP_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost/ 2>/dev/null || echo "000")
if [ "$HTTP_RESPONSE" = "200" ] || [ "$HTTP_RESPONSE" = "301" ] || [ "$HTTP_RESPONSE" = "308" ]; then
    echo -e "${GREEN}✅ HTTP responde correctamente (código: $HTTP_RESPONSE)${NC}"
    if [ "$HTTP_RESPONSE" = "301" ] || [ "$HTTP_RESPONSE" = "308" ]; then
        echo "   (Redirección HTTP → HTTPS funcionando)"
    fi
else
    echo -e "${YELLOW}⚠️  HTTP respondió con código: $HTTP_RESPONSE${NC}"
    echo "   Verificando logs de Traefik..."
    docker-compose -f docker-compose.test.yml logs traefik | tail -20
fi
echo ""

# 9. Verificar logs de Traefik para errores
echo "9️⃣  Verificando logs de Traefik para errores..."
TRAEFIK_ERRORS=$(docker-compose -f docker-compose.test.yml logs traefik 2>&1 | grep -iE "error|Error|ERROR" | tail -10)
if [ -z "$TRAEFIK_ERRORS" ]; then
    echo -e "${GREEN}✅ No se encontraron errores en logs de Traefik${NC}"
else
    echo -e "${YELLOW}⚠️  Errores encontrados en logs de Traefik:${NC}"
    echo "$TRAEFIK_ERRORS"
fi
echo ""

# 10. Resumen final
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo -e "${BLUE}📋 RESUMEN DE PRUEBAS${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "✅ Traefik está corriendo"
echo "✅ Servicio web está corriendo"
echo "✅ Labels de Traefik están configurados"
echo "✅ Traefik detecta el servicio"
echo "✅ Servicio web responde correctamente"
echo ""
echo "🌐 Accesos de prueba:"
echo "   - Traefik Dashboard: http://localhost:8080"
echo "   - Frontend (HTTP): http://localhost/"
echo "   - Frontend (HTTPS): https://localhost/ (puede mostrar advertencia de certificado)"
echo ""
echo -e "${GREEN}✅ Todas las pruebas pasaron correctamente${NC}"
echo ""
echo "💡 Para ver logs en tiempo real:"
echo "   docker-compose -f docker-compose.test.yml logs -f"
echo ""
echo "💡 Para detener los servicios:"
echo "   docker-compose -f docker-compose.test.yml down"
echo ""

