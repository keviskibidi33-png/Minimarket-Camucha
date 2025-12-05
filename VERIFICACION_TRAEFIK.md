# 🔍 Guía de Verificación de Traefik

## ✅ Verificación Rápida

### 1. Verificar que Traefik esté corriendo

```bash
# En el servidor de producción
docker ps | grep traefik
```

**Resultado esperado**: Debe mostrar el contenedor de Traefik corriendo.

---

### 2. Verificar que el servicio web esté corriendo

```bash
docker ps | grep minimarket-web
```

**Resultado esperado**: Debe mostrar el contenedor `minimarket-web` corriendo.

---

### 3. Verificar labels de Traefik en el contenedor web

```bash
# Obtener nombre del contenedor
CONTAINER_NAME=$(docker ps --format "{{.Names}}" | grep minimarket-web | head -1)

# Ver todos los labels de Traefik
docker inspect $CONTAINER_NAME --format '{{range $key, $value := .Config.Labels}}{{$key}}={{$value}}{{"\n"}}{{end}}' | grep traefik
```

**Labels críticos que deben estar presentes**:

```bash
traefik.enable=true
traefik.http.routers.web.rule=Host(`minimarket.edvio.app`)
traefik.http.routers.web.entrypoints=websecure
traefik.http.routers.web.tls=true
traefik.http.routers.web.tls.certresolver=letsencrypt  # ⚠️ Debe ser 'letsencrypt', NO 'myresolver'
traefik.http.services.web.loadbalancer.server.port=80
```

---

### 4. Verificar conectividad del servicio web

```bash
CONTAINER_NAME=$(docker ps --format "{{.Names}}" | grep minimarket-web | head -1)
docker exec $CONTAINER_NAME wget --quiet --tries=1 --spider http://localhost/
```

**Resultado esperado**: Exit code 0 (éxito).

---

### 5. Verificar logs de Traefik

```bash
TRAEFIK_CONTAINER=$(docker ps --format "{{.Names}}" | grep traefik | head -1)
docker logs --tail 50 $TRAEFIK_CONTAINER | grep -E "minimarket|error|Error|ERROR"
```

**Buscar**:
- ✅ `minimarket.edvio.app` en los logs
- ✅ `Certificate obtained` o `Certificate renewed`
- ❌ NO debe haber errores relacionados con `minimarket`

---

### 6. Verificar DNS

```bash
dig minimarket.edvio.app +short
# O
nslookup minimarket.edvio.app
```

**Resultado esperado**: `103.138.188.233`

---

### 7. Verificar certificado SSL

```bash
echo | openssl s_client -connect minimarket.edvio.app:443 -servername minimarket.edvio.app 2>/dev/null | grep -E "Issuer|subject="
```

**Resultado esperado**: 
- ✅ `Issuer: C=US, O=Let's Encrypt, CN=R3`
- ❌ NO debe mostrar `Traefik Default Certificate`

---

### 8. Probar el dominio directamente

```bash
curl -I https://minimarket.edvio.app
```

**Resultado esperado**: 
```
HTTP/2 200
```

Si obtienes `404 Not Found`, significa que Traefik no está enrutando correctamente.

---

## 🚨 Problemas Comunes y Soluciones

### Problema 1: Error 404 "page not found"

**Causas posibles**:
1. Labels de Traefik incorrectos
2. CertResolver incorrecto (`myresolver` en lugar de `letsencrypt`)
3. Nombre del router conflictivo (`web` en lugar de `minimarket`)
4. Puerto incorrecto en `loadbalancer.server.port`

**Solución**:
1. Verifica que `coolify.yml` use:
   - `certresolver=letsencrypt` (NO `myresolver`)
   - Nombre único para router (ej: `minimarket`)
   - `loadbalancer.server.port=80`

2. Verifica labels en el contenedor:
   ```bash
   docker inspect <web-container> --format '{{index .Config.Labels "traefik.http.routers.web.tls.certresolver"}}'
   ```

3. Reinicia el servicio web en Coolify

---

### Problema 2: Certificado inválido (ERR_CERT_AUTHORITY_INVALID)

**Causa**: Traefik está usando certificado por defecto en lugar de Let's Encrypt

**Solución**:
1. Verifica que `certresolver=letsencrypt` esté configurado
2. Verifica que el dominio apunte correctamente a `103.138.188.233`
3. Espera 5-10 minutos después del despliegue para que Let's Encrypt genere el certificado
4. Verifica logs de Traefik para errores de ACME

---

### Problema 3: Traefik no detecta el servicio

**Causa**: Labels de Traefik incorrectos o faltantes

**Solución**:
1. Verifica que `traefik.enable=true` esté presente
2. Verifica que el servicio use `expose: - "80"` (NO `ports:`)
3. Verifica que el servicio esté en la misma red que Traefik
4. Reinicia Traefik si es necesario

---

## 📋 Checklist de Verificación

Antes de considerar que Traefik funciona correctamente:

- [ ] Traefik está corriendo
- [ ] Servicio web está corriendo
- [ ] Labels de Traefik están configurados correctamente
- [ ] `certresolver=letsencrypt` (NO `myresolver`)
- [ ] `loadbalancer.server.port=80`
- [ ] DNS resuelve correctamente
- [ ] Certificado SSL válido de Let's Encrypt
- [ ] `https://minimarket.edvio.app` responde con HTTP/2 200
- [ ] No hay errores en logs de Traefik relacionados con `minimarket`

---

## 🛠️ Scripts de Verificación Automática

### Script Bash (Linux/Mac)

```bash
chmod +x verificar-traefik.sh
./verificar-traefik.sh
```

### Script PowerShell (Windows)

```powershell
.\verificar-traefik.ps1
```

Estos scripts verifican automáticamente todos los puntos anteriores.

---

## 📊 Verificación desde Coolify

Si tienes acceso SSH a través de Coolify:

1. Ve a tu aplicación en Coolify
2. Sección **"Terminal"** o **"SSH"**
3. Ejecuta los comandos de verificación arriba

---

## 🔍 Verificación Avanzada

### Ver routers de Traefik (si el dashboard está habilitado)

```bash
curl http://localhost:8080/api/http/routers 2>/dev/null | grep minimarket
```

### Ver servicios de Traefik

```bash
curl http://localhost:8080/api/http/services 2>/dev/null | grep minimarket
```

### Ver entrypoints de Traefik

```bash
curl http://localhost:8080/api/http/entrypoints 2>/dev/null
```

---

## ✅ Resultado Esperado

Después de verificar todo:

- ✅ Traefik está corriendo y detecta el servicio `web`
- ✅ Labels están configurados correctamente
- ✅ Certificado SSL válido de Let's Encrypt
- ✅ `https://minimarket.edvio.app` carga correctamente
- ✅ No hay errores 404
- ✅ Redirección HTTP → HTTPS funciona

**¡Traefik está funcionando correctamente!** 🎉

