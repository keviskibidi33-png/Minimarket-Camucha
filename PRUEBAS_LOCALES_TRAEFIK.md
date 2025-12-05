# 🧪 Guía de Pruebas Locales de Traefik

## 📋 Prerequisitos

1. **Docker Desktop debe estar corriendo**
   - Verifica que Docker Desktop esté iniciado
   - Espera a que esté completamente listo (ícono verde)

2. **Puertos disponibles**:
   - Puerto 80 (HTTP)
   - Puerto 443 (HTTPS)
   - Puerto 8080 (Traefik Dashboard)

---

## 🚀 Ejecutar Pruebas

### Opción 1: Script Automático (Recomendado)

#### Windows (PowerShell):
```powershell
# Asegúrate de que Docker Desktop esté corriendo primero
.\test-traefik-local.ps1
```

#### Linux/Mac (Bash):
```bash
# Asegúrate de que Docker esté corriendo primero
chmod +x test-traefik-local.sh
./test-traefik-local.sh
```

### Opción 2: Manual

```bash
# 1. Construir y levantar servicios
docker-compose -f docker-compose.test.yml up -d --build

# 2. Esperar a que los servicios estén listos (2-3 minutos)
# Verificar logs:
docker-compose -f docker-compose.test.yml logs -f

# 3. Verificar que Traefik esté corriendo
docker ps | grep traefik-test

# 4. Verificar que el servicio web esté corriendo
docker ps | grep minimarket-web-test

# 5. Verificar labels de Traefik
docker inspect minimarket-web-test --format '{{range $key, $value := .Config.Labels}}{{$key}}={{$value}}{{"\n"}}{{end}}' | grep traefik

# 6. Probar acceso HTTP
curl -I http://localhost/

# 7. Acceder al Dashboard de Traefik
# Abre en navegador: http://localhost:8080

# 8. Detener servicios cuando termines
docker-compose -f docker-compose.test.yml down -v
```

---

## ✅ Qué Validar

### 1. Traefik está corriendo
```bash
docker ps | grep traefik-test
```
**Resultado esperado**: Contenedor `traefik-test` corriendo

### 2. Servicio web está corriendo
```bash
docker ps | grep minimarket-web-test
```
**Resultado esperado**: Contenedor `minimarket-web-test` corriendo

### 3. Labels de Traefik correctos
```bash
docker inspect minimarket-web-test --format '{{index .Config.Labels "traefik.enable"}}'
```
**Resultado esperado**: `true`

```bash
docker inspect minimarket-web-test --format '{{index .Config.Labels "traefik.http.routers.web.tls.certresolver"}}'
```
**Resultado esperado**: `letsencrypt` (o `myresolver` si estás probando)

### 4. Traefik detecta el servicio
- Abre: http://localhost:8080
- Ve a "HTTP" → "Routers"
- Debe aparecer un router llamado `web`

### 5. HTTP responde correctamente
```bash
curl -I http://localhost/
```
**Resultado esperado**: 
- `HTTP/1.1 200 OK` (si funciona directamente)
- `HTTP/1.1 301 Moved Permanently` o `HTTP/1.1 308 Permanent Redirect` (si redirige a HTTPS)

### 6. Servicio web responde internamente
```bash
docker exec minimarket-web-test wget --quiet --tries=1 --spider http://localhost/
```
**Resultado esperado**: Exit code 0 (éxito)

---

## 🔍 Verificar Logs

### Logs de Traefik:
```bash
docker-compose -f docker-compose.test.yml logs traefik
```

### Logs del servicio web:
```bash
docker-compose -f docker-compose.test.yml logs web
```

### Logs de todos los servicios:
```bash
docker-compose -f docker-compose.test.yml logs -f
```

---

## 🐛 Problemas Comunes

### Error: "Docker Desktop no está corriendo"
**Solución**: Inicia Docker Desktop y espera a que esté completamente listo.

### Error: "Port is already allocated"
**Solución**: 
- Verifica que no haya otros servicios usando los puertos 80, 443, 8080
- Detén otros contenedores: `docker ps` y luego `docker stop <container-id>`

### Error: "Traefik no detecta el servicio"
**Solución**:
- Verifica que `traefik.enable=true` esté en los labels
- Verifica que el servicio use `expose: - "80"` (NO `ports:`)
- Espera 10-20 segundos después de que el servicio inicie

### Error: "Service web no responde"
**Solución**:
- Verifica logs: `docker-compose -f docker-compose.test.yml logs web`
- Verifica que Nginx esté corriendo: `docker exec minimarket-web-test ps aux | grep nginx`

---

## 📊 Resultado Esperado

Si todas las pruebas pasan:

- ✅ Traefik está corriendo y detecta el servicio `web`
- ✅ Labels de Traefik están configurados correctamente
- ✅ HTTP responde correctamente (200 o redirección 301/308)
- ✅ Servicio web responde internamente
- ✅ No hay errores en logs de Traefik

**Si todas las pruebas pasan → La configuración está lista para producción** ✅

---

## 🧹 Limpiar Después de las Pruebas

```bash
# Detener y eliminar contenedores y volúmenes
docker-compose -f docker-compose.test.yml down -v

# Verificar que todo esté limpio
docker ps -a | grep -E "traefik-test|minimarket-web-test|minimarket-api-test|minimarket-db-test"
```

---

## 📝 Notas Importantes

1. **Este entorno de prueba simula Coolify** pero usa `localhost` en lugar de `minimarket.edvio.app`
2. **Los certificados SSL serán autofirmados** (mostrarán advertencia en el navegador)
3. **Los datos de prueba se eliminan** cuando ejecutas `docker-compose down -v`
4. **Este entorno NO afecta** tu configuración de producción

---

## ✅ Checklist Pre-Commit

Antes de hacer commit y push, verifica:

- [ ] Docker Desktop está corriendo
- [ ] Todas las pruebas pasan localmente
- [ ] Traefik detecta el servicio correctamente
- [ ] HTTP responde correctamente
- [ ] No hay errores en logs
- [ ] Labels de Traefik están correctos en `coolify.yml`

**Solo después de que todas las pruebas pasen, haz commit y push** 🚀

