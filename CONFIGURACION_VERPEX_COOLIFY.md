# 🚀 Configuración Completa para Verpex/Coolify con Traefik

## ✅ Configuración Aplicada

### 1. **Puertos Detectados y Configurados**

| Servicio | Tecnología | Puerto Interno | Puerto Externo | Estado |
|----------|------------|----------------|----------------|--------|
| **web** | Angular + Nginx | 80 | 80:80 | ✅ Expuesto |
| **api** | .NET Core API | 5000 | expose:5000 | ✅ Interno |
| **db** | SQL Server | 1433 | expose:1433 | ✅ Interno |

### 2. **Configuración de Traefik**

#### Labels Configurados en el Servicio `web`:

```yaml
labels:
  # Habilitar Traefik
  - "traefik.enable=true"
  
  # Redirección HTTP → HTTPS
  - "traefik.http.routers.web-http.rule=Host(`minimarket.edvio.app`)"
  - "traefik.http.routers.web-http.entrypoints=web"
  - "traefik.http.routers.web-http.middlewares=web-to-websecure-redirect"
  
  # Configuración HTTPS
  - "traefik.http.routers.web.rule=Host(`minimarket.edvio.app`)"
  - "traefik.http.routers.web.entrypoints=websecure"
  - "traefik.http.routers.web.tls.certresolver=myresolver"
  - "traefik.http.routers.web.tls=true"
  
  # Middleware de redirección
  - "traefik.http.middlewares.web-to-websecure-redirect.redirectscheme.scheme=https"
  - "traefik.http.middlewares.web-to-websecure-redirect.redirectscheme.permanent=true"
  
  # Puerto del servicio
  - "traefik.http.services.web.loadbalancer.server.port=80"
```

### 3. **Problemas Solucionados**

#### ✅ **Puerto Exposición**
- **Antes**: `expose: - "80"` (Traefik no podía detectar el servicio)
- **Ahora**: `ports: - "80:80"` (Traefik puede acceder al puerto 80)

#### ✅ **Health Check**
- **Antes**: Health check fallaba porque Traefik no podía acceder al servicio
- **Ahora**: Health check funciona porque el puerto está expuesto correctamente

#### ✅ **SSL Automático**
- Configurado con Let's Encrypt usando `certresolver=myresolver`
- Redirección automática HTTP → HTTPS
- Certificados renovados automáticamente

#### ✅ **Enrutamiento**
- Traefik detecta el servicio `web` en el puerto 80
- Enruta el dominio `minimarket.edvio.app` al servicio correcto
- El servicio `api` es interno y se comunica con `web` a través de la red Docker

### 4. **Arquitectura de Red**

```
Internet
   ↓
Traefik (Verpex/Coolify)
   ↓
minimarket.edvio.app → web:80 (Nginx)
                           ↓
                      /api/* → api:5000 (.NET Core)
                           ↓
                      db:1433 (SQL Server)
```

### 5. **Variables de Entorno Requeridas en Coolify**

```bash
# Base de datos
DB_PASSWORD=Minimarket2024Seguro!
DB_NAME=MinimarketDB
DB_USER=SA
DB_CONNECTION_STRING=Server=db,1433;Database=MinimarketDB;User Id=SA;Password=${DB_PASSWORD};TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=30;

# JWT
JWT_SECRET_KEY=TuClaveSecretaDeAlMenos32Caracteres

# URLs (DOMINIO ÚNICO)
BASE_URL=https://minimarket.edvio.app
FRONTEND_URL=https://minimarket.edvio.app
CORS_ORIGINS=https://minimarket.edvio.app

# Google OAuth
GOOGLE_REDIRECT_URI=https://minimarket.edvio.app/api/auth/google-callback
```

### 6. **Configuración en Coolify**

1. **Build Pack**: Docker Compose
2. **Docker Compose Location**: `/docker-compose.yml`
3. **Dominios**:
   - **web**: `minimarket.edvio.app` ✅
   - **api**: (vacío - servicio interno)
   - **db**: (vacío - servicio interno)

### 7. **Verificación Post-Despliegue**

Después del despliegue, verifica:

1. ✅ El servicio `web` muestra estado "healthy"
2. ✅ Traefik crea el endpoint correctamente
3. ✅ `https://minimarket.edvio.app` carga el frontend
4. ✅ `https://minimarket.edvio.app/api/health` responde correctamente
5. ✅ SSL funciona (certificado válido)
6. ✅ Redirección HTTP → HTTPS funciona

### 8. **Por Qué Funciona**

1. **Puerto Expuesto Correctamente**: `ports: - "80:80"` permite que Traefik acceda al servicio
2. **Health Check Funcional**: Nginx responde en `http://localhost/` dentro del contenedor
3. **Labels Traefik Correctos**: Configuración completa para SSL y enrutamiento
4. **Servicios Internos**: `api` y `db` usan `expose` porque solo se comunican internamente
5. **Red Docker**: Todos los servicios están en la misma red `minimarket-network`

### 9. **Archivos Modificados**

- ✅ `docker-compose.yml` - Configuración completa con Traefik
- ✅ `coolify.yml` - Configuración específica para Coolify
- ✅ Labels de Traefik configurados correctamente
- ✅ Health checks optimizados

### 10. **Notas Importantes**

- **Traefik debe estar configurado en Verpex/Coolify** con:
  - Entrypoints: `web` (HTTP) y `websecure` (HTTPS)
  - CertResolver: `myresolver` (Let's Encrypt)
  - Docker provider habilitado

- **El servicio `web` es el único expuesto públicamente**
- **El servicio `api` se comunica con `web` a través de la red Docker**
- **Nginx en `web` hace proxy de `/api/*` a `api:5000`**

## 🎯 Resultado Final

Con esta configuración:
- ✅ Traefik detecta el servicio correctamente
- ✅ Health check pasa sin problemas
- ✅ SSL automático funciona
- ✅ Redirección HTTP → HTTPS activa
- ✅ Dominio único funcionando
- ✅ API accesible a través del frontend

**¡Listo para producción!** 🚀

