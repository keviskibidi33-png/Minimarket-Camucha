# Solución: Error de Timeout al Descargar Imágenes Docker

## 🔴 Error Encontrado

```
failed to resolve source metadata for docker.io/library/node:20: 
failed to do request: Head "https://registry-1.docker.io/v2/library/node/manifests/20": 
net/http: TLS handshake timeout
```

## 📋 Causa del Problema

El servidor no puede conectarse a Docker Hub para descargar las imágenes base. Esto puede deberse a:

1. **Problemas de red temporales** en el servidor
2. **Firewall bloqueando** conexiones a Docker Hub
3. **Docker Hub está lento o caído** temporalmente
4. **Problemas de DNS** en el servidor

## ✅ Soluciones Aplicadas

### 1. Cambio a Imágenes Alpine (Más Pequeñas)

He cambiado el Dockerfile para usar imágenes Alpine que son:
- **Más pequeñas** (descarga más rápida)
- **Más estables** (versiones específicas)
- **Menos propensas a timeouts** (menor tamaño = menos tiempo de descarga)

**Cambios realizados:**
- `node:20` → `node:20-alpine`
- `nginx:alpine` → `nginx:1.27-alpine`

### 2. Configuración de Reintentos en npm

Agregado configuración para que npm reintente automáticamente si hay problemas de red:

```dockerfile
RUN npm config set fetch-retries 5 && \
    npm config set fetch-retry-mintimeout 20000 && \
    npm config set fetch-retry-maxtimeout 120000
```

## 🔧 Soluciones Adicionales (Si el Problema Persiste)

### Opción 1: Reintentar el Despliegue

El problema puede ser temporal. Simplemente:
1. Espera 5-10 minutos
2. Haz clic en **"Redeploy"** en Coolify
3. El build debería funcionar si el problema era temporal

### Opción 2: Verificar Conectividad del Servidor

Si tienes acceso SSH al servidor, verifica:

```bash
# Verificar que el servidor puede conectarse a Docker Hub
curl -I https://registry-1.docker.io/v2/

# Verificar DNS
nslookup registry-1.docker.io

# Verificar conectividad
ping registry-1.docker.io
```

### Opción 3: Configurar Proxy en Docker (Si Aplica)

Si el servidor está detrás de un proxy, configura Docker para usarlo:

```bash
# En el servidor
sudo mkdir -p /etc/systemd/system/docker.service.d
sudo nano /etc/systemd/system/docker.service.d/http-proxy.conf
```

Agregar:
```ini
[Service]
Environment="HTTP_PROXY=http://proxy.example.com:8080"
Environment="HTTPS_PROXY=http://proxy.example.com:8080"
Environment="NO_PROXY=localhost,127.0.0.1"
```

Luego:
```bash
sudo systemctl daemon-reload
sudo systemctl restart docker
```

### Opción 4: Usar Mirror de Docker Hub

Si Docker Hub está bloqueado, puedes configurar un mirror. Esto requiere acceso a la configuración de Docker en el servidor.

### Opción 5: Pre-descargar Imágenes

Si tienes acceso al servidor, puedes pre-descargar las imágenes:

```bash
docker pull node:20-alpine
docker pull nginx:1.27-alpine
docker pull mcr.microsoft.com/dotnet/sdk:9.0
docker pull mcr.microsoft.com/dotnet/aspnet:9.0
```

## 📝 Verificación

Después de aplicar los cambios:

1. **Haz commit y push** de los cambios
2. **Espera 5-10 minutos** (por si el problema era temporal)
3. **Haz clic en "Redeploy"** en Coolify
4. **Verifica los logs** para ver si el problema persiste

## 🚨 Si el Problema Persiste

Si después de reintentar el problema continúa:

1. **Verifica el estado de Docker Hub**: https://status.docker.com/
2. **Contacta al administrador del servidor** para verificar:
   - Conectividad a internet
   - Configuración de firewall
   - Configuración de proxy
3. **Considera usar un registry alternativo** (si está disponible)

## 📌 Nota Importante

Este es un problema de **infraestructura/red**, no del código. Los cambios que hice optimizan el proceso, pero si el servidor no puede conectarse a Docker Hub, el build seguirá fallando.

La solución más común es **simplemente esperar y reintentar**, ya que muchos problemas de red son temporales.

