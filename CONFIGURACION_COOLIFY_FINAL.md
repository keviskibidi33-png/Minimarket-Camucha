# 🎯 Configuración Final para Coolify - Lista para Producción

## ✅ Resumen de Correcciones Aplicadas

### Problemas Críticos Corregidos:

1. ✅ **CertResolver**: `myresolver` → `letsencrypt` (obligatorio para Coolify)
2. ✅ **Nombre del Router**: `web` → `minimarket` (evita conflictos con Traefik/Coolify)
3. ✅ **Servicio DB**: Agregado a `coolify.yml` (faltaba completamente)
4. ✅ **Dependencias**: `depends_on` configurado correctamente
5. ✅ **Puertos**: Usando `expose:` (correcto para Coolify)
6. ✅ **Connection String**: Configurado con `Server=db,1433`

---

## 📋 Configuración en Coolify

### Paso 1: Build Pack y Archivo

1. Ve a tu aplicación en Coolify
2. Sección **"General"**
3. **Build Pack**: `Docker Compose`
4. **Docker Compose Location**: `/coolify.yml`
5. **Base Directory**: `/` (raíz del repositorio)

### Paso 2: Dominios (CRÍTICO)

En la sección **"General"** > **"Domains"**:

| Servicio | Dominio | Estado |
|----------|---------|--------|
| **db** | (vacío) | ✅ Servicio interno |
| **api** | (vacío) | ✅ Servicio interno |
| **web** | `minimarket.edvio.app` | ✅ Único dominio público |

**⚠️ IMPORTANTE**: 
- NO agregues `https://` - solo `minimarket.edvio.app`
- Coolify maneja HTTPS automáticamente con Traefik

### Paso 3: Internal Port (CRÍTICO)

En el servicio **web**, configura:

**Internal Port**: `80`

Esto le dice a Traefik en qué puerto interno está escuchando el servicio.

### Paso 4: Variables de Entorno (OBLIGATORIAS)

Ve a **"Environment Variables"** y configura:

```bash
# ============================================
# BASE DE DATOS (OBLIGATORIAS)
# ============================================
DB_PASSWORD=Minimarket2024Seguro!
DB_NAME=MinimarketDB
DB_USER=SA

# ============================================
# JWT AUTHENTICATION (OBLIGATORIA)
# ============================================
JWT_SECRET_KEY=TuClaveSecretaDeAlMenos32CaracteresMuyLargaYSegura123456789

# ============================================
# URLs Y CORS (OBLIGATORIAS - DOMINIO ÚNICO)
# ============================================
BASE_URL=https://minimarket.edvio.app
FRONTEND_URL=https://minimarket.edvio.app
CORS_ORIGINS=https://minimarket.edvio.app

# ============================================
# GOOGLE OAUTH (OBLIGATORIA)
# ============================================
GOOGLE_REDIRECT_URI=https://minimarket.edvio.app/api/auth/google-callback

# ============================================
# OPCIONALES (tienen valores por defecto)
# ============================================
# API_URL=/api
# SMTP_SERVER=smtp.gmail.com
# SMTP_PORT=587
# SMTP_USER=minimarket.camucha@gmail.com
# SMTP_PASSWORD=xzloatedigfqgyxi
# FROM_EMAIL=minimarket.camucha@gmail.com
# FROM_NAME=Minimarket Camucha
```

### Paso 5: Orden de Despliegue

Coolify debe desplegar en este orden:

1. **db** (primero - base de datos)
2. **api** (segundo - espera a que db esté healthy)
3. **web** (tercero - espera a que api esté listo)

Esto está configurado automáticamente con `depends_on`.

---

## 🌐 Verificación DNS

### Registros DNS Requeridos

En tu panel de DNS de Verpex (o donde tengas `edvio.app`):

```
Tipo  Nombre       Valor              TTL
A     @           103.138.188.233    3600
A     *           103.138.188.233    3600
A     minimarket  103.138.188.233    3600
```

### Verificación DNS

```bash
# Verificar resolución DNS
nslookup minimarket.edvio.app
# Debe devolver: 103.138.188.233

# Verificar desde terminal
dig minimarket.edvio.app +short
# Debe devolver: 103.138.188.233

# Verificar conectividad
ping minimarket.edvio.app
# Debe responder desde 103.138.188.233
```

---

## 🔐 Configuración de Google OAuth

### Actualizar Google Cloud Console

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. **APIs & Services** > **Credentials**
3. Edita el OAuth 2.0 Client ID: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e`

### Authorized JavaScript origins

```
https://minimarket.edvio.app
http://localhost:4200
https://localhost:4200
```

### Authorized redirect URIs

```
https://minimarket.edvio.app/api/auth/google-callback
http://localhost:5000/api/auth/google-callback
https://localhost:5000/api/auth/google-callback
```

---

## ✅ Verificación Post-Despliegue

### 1. Estado de Contenedores

En Coolify, verifica que todos estén **"Running (healthy)"**:

- ✅ `db` - Estado: Healthy
- ✅ `api` - Estado: Healthy  
- ✅ `web` - Estado: Healthy

### 2. Verificar Certificado SSL

```bash
# Verificar certificado SSL
openssl s_client -connect minimarket.edvio.app:443 -servername minimarket.edvio.app | grep "Issuer"

# Debe mostrar: "Issuer: C=US, O=Let's Encrypt, CN=R3"
# NO debe mostrar: "Traefik Default Certificate"
```

**En el navegador**:
1. Abre `https://minimarket.edvio.app`
2. Click en el candado → "Ver certificado"
3. Debe mostrar: **"Let's Encrypt"** o **"R3"**
4. NO debe mostrar: "Traefik Default Certificate" o "Self-signed"

### 3. Verificar Endpoints

```bash
# Frontend
curl -I https://minimarket.edvio.app
# Debe devolver: HTTP/2 200

# API Health Check
curl https://minimarket.edvio.app/api/health
# Debe devolver: {"status":"Healthy",...}

# Verificar redirección HTTP → HTTPS
curl -I http://minimarket.edvio.app
# Debe devolver: HTTP/1.1 301 o 308 (redirección a HTTPS)
```

### 4. Verificar Logs de Traefik

En Coolify, ve a **"Logs"** del servicio Traefik y busca:

```
✅ "Certificate obtained" o "Certificate renewed"
✅ "minimarket.edvio.app" en los logs
✅ Sin errores de "certificate" o "ACME"
✅ Router "minimarket" creado correctamente
```

---

## 🔍 Troubleshooting del Error 404

### Si Traefik muestra 404 "page not found"

**Posibles causas**:

1. **Labels incorrectos**: Verifica que `traefik.enable=true` esté presente
2. **Nombre del router conflictivo**: Verifica que use `minimarket` (no `web`)
3. **Puerto incorrecto**: Verifica que `loadbalancer.server.port=80` coincida con `expose: - "80"`
4. **Dominio no configurado**: Verifica que `minimarket.edvio.app` esté en la sección Domains de Coolify
5. **Internal Port no configurado**: Verifica que Internal Port = 80 en el servicio web

**Solución paso a paso**:

1. Verifica logs de Traefik: Busca errores relacionados con `minimarket.edvio.app`
2. Verifica que el servicio `web` esté corriendo: `docker ps | grep web`
3. Verifica que el puerto 80 esté expuesto: `docker exec <web-container> netstat -tlnp | grep 80`
4. Verifica labels: `docker inspect <web-container> | grep -A 20 Labels`
5. Verifica que Traefik detecte el servicio: Busca en logs de Traefik `minimarket`

---

## 📊 Arquitectura Final

```
Internet
   ↓
DNS: minimarket.edvio.app → 103.138.188.233
   ↓
Traefik (Coolify) - Puerto 80/443 externo
   ↓
Labels detectan: Host(`minimarket.edvio.app`)
   ↓
Router: minimarket → websecure → TLS (letsencrypt)
   ↓
Servicio web:80 (Nginx + Angular) - expose:80
   ↓
   ├─ / → index.html (Angular SPA)
   └─ /api/* → api:5000 (.NET Core API) - expose:5000
                  ↓
              db:1433 (SQL Server) - expose:1433
```

---

## 🎯 Checklist Final Pre-Despliegue

Antes de desplegar, verifica:

- [ ] DNS configurado: `minimarket.edvio.app` → `103.138.188.233`
- [ ] `coolify.yml` usa `letsencrypt` (no `myresolver`)
- [ ] `coolify.yml` usa `minimarket` en routers (no `web`)
- [ ] `coolify.yml` incluye servicio `db` completo
- [ ] `coolify.yml` tiene `depends_on` configurado
- [ ] Variables de entorno configuradas en Coolify
- [ ] Google OAuth configurado con `https://minimarket.edvio.app`
- [ ] Health checks configurados correctamente
- [ ] `expose: - "80"` configurado en servicio `web`
- [ ] Internal Port = 80 configurado en Coolify para servicio `web`
- [ ] Dominio `minimarket.edvio.app` configurado en Coolify para servicio `web`

---

## 📝 Archivos Finales

- ✅ `coolify.yml` - Configuración completa y corregida
- ✅ `docker-compose.yml` - Para desarrollo local
- ✅ Labels de Traefik corregidos y optimizados
- ✅ Health checks optimizados
- ✅ Connection string configurado correctamente

---

## 🚀 Resultado Esperado

Después de aplicar esta configuración:

- ✅ `https://minimarket.edvio.app` carga el frontend (sin 404)
- ✅ `https://minimarket.edvio.app/api/health` responde correctamente
- ✅ Certificado SSL válido de Let's Encrypt
- ✅ Sin errores de certificado en Chrome
- ✅ Redirección HTTP → HTTPS automática
- ✅ Todos los servicios en estado "Healthy"
- ✅ Traefik genera certificados automáticamente
- ✅ Sin errores 404 de Traefik

**¡Listo para producción!** 🎉

