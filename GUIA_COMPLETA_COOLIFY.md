# 🚀 Guía Completa de Configuración para Coolify + Traefik + Verpex

## ✅ Configuración Final Aplicada

### 📋 Problemas Corregidos

1. ✅ **CertResolver**: Cambiado de `myresolver` → `letsencrypt` (obligatorio para Coolify)
2. ✅ **Nombre del Router**: Cambiado de `web` → `minimarket` (evita conflictos con Traefik)
3. ✅ **Servicio DB**: Agregado a `coolify.yml` (faltaba)
4. ✅ **Puertos**: Usando `expose:` en lugar de `ports:` (correcto para Coolify)
5. ✅ **Health Checks**: Optimizados y corregidos
6. ✅ **Connection String**: Configurado correctamente con `Server=db,1433`

---

## 🔧 Configuración en Coolify

### Paso 1: Configurar Build Pack

1. Ve a tu aplicación en Coolify
2. Sección **"General"**
3. **Build Pack**: Selecciona `Docker Compose`
4. **Docker Compose Location**: `/coolify.yml` o `/docker-compose.yml`
   - **Recomendación**: Usa `/coolify.yml` (archivo optimizado para Coolify)

### Paso 2: Configurar Dominios

En la sección **"General"** > **"Domains"**:

- **Domains for db**: (vacío - servicio interno)
- **Domains for app**: (vacío - servicio interno)  
- **Domains for web**: `minimarket.edvio.app` ✅

### Paso 3: Variables de Entorno (CRÍTICO)

Ve a **"Environment Variables"** y configura estas variables:

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

**⚠️ IMPORTANTE**: 
- `DB_PASSWORD` debe cumplir requisitos de SQL Server (mínimo 8 caracteres, mayúsculas, minúsculas, números, especiales)
- `JWT_SECRET_KEY` debe tener mínimo 32 caracteres (recomendado 64+)
- Todas las URLs deben usar `https://` (no `http://`)

---

## 🌐 Configuración DNS en Verpex

### Registros DNS Requeridos

En tu panel de DNS de Verpex (o donde tengas configurado `edvio.app`), asegúrate de tener:

```
Tipo  Nombre              Valor              TTL
A     @                   103.138.188.233    3600
A     *                   103.138.188.233    3600
A     minimarket          103.138.188.233    3600
```

**Nota**: El registro `A *` (wildcard) permite que cualquier subdominio apunte al servidor, pero el específico `minimarket` tiene prioridad.

### Verificación DNS

Después de configurar DNS, verifica con:

```bash
# Verificar resolución DNS
nslookup minimarket.edvio.app
# Debe devolver: 103.138.188.233

# Verificar desde terminal
dig minimarket.edvio.app +short
# Debe devolver: 103.138.188.233
```

---

## 🔐 Configuración de Google OAuth

### Paso 1: Actualizar Google Cloud Console

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona tu proyecto
3. Ve a **APIs & Services** > **Credentials**
4. Edita el OAuth 2.0 Client ID: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e`

### Paso 2: Authorized JavaScript origins

Agrega:
```
https://minimarket.edvio.app
http://localhost:4200
https://localhost:4200
```

### Paso 3: Authorized redirect URIs

Agrega:
```
https://minimarket.edvio.app/api/auth/google-callback
http://localhost:5000/api/auth/google-callback
https://localhost:5000/api/auth/google-callback
```

### Paso 4: Guardar y Esperar

- Haz clic en **"Save"**
- Espera 5-10 minutos para que los cambios se propaguen

---

## ✅ Verificación Post-Despliegue

### 1. Verificar Estado de Contenedores

En Coolify, verifica que todos los servicios estén **"Running (healthy)"**:

- ✅ `db` - Estado: Healthy
- ✅ `api` - Estado: Healthy  
- ✅ `web` - Estado: Healthy

### 2. Verificar Certificado SSL

```bash
# Verificar certificado SSL
openssl s_client -connect minimarket.edvio.app:443 -servername minimarket.edvio.app

# O usar navegador
# Abre: https://minimarket.edvio.app
# Click en el candado → Ver certificado
# Debe mostrar: "Let's Encrypt" o "R3"
```

**✅ Certificado Correcto**: Debe mostrar "Let's Encrypt" o "R3"  
**❌ Certificado Incorrecto**: Muestra "Traefik Default Certificate" o "Self-signed"

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
# Debe devolver: HTTP/1.1 301 o 308 (redirección)
```

### 4. Verificar Logs de Traefik

En Coolify, ve a **"Logs"** del servicio Traefik y busca:

```
✅ "Certificate obtained" o "Certificate renewed"
✅ "minimarket.edvio.app" en los logs
✅ Sin errores de "certificate" o "ACME"
```

---

## 🔍 Troubleshooting

### Problema: Certificado inválido (ERR_CERT_AUTHORITY_INVALID)

**Causa**: Traefik está usando certificado por defecto en lugar de Let's Encrypt

**Solución**:
1. Verifica que `certresolver=letsencrypt` (no `myresolver`)
2. Verifica que el dominio `minimarket.edvio.app` apunta correctamente a `103.138.188.233`
3. Verifica que Traefik puede acceder al puerto 80/443 desde internet
4. Espera 5-10 minutos después del despliegue para que Let's Encrypt genere el certificado

### Problema: Health Check Falla

**Causa**: El servicio no responde correctamente

**Solución**:
1. Verifica logs del servicio: `docker logs minimarket-web`
2. Verifica que Nginx esté corriendo: `docker exec minimarket-web ps aux | grep nginx`
3. Verifica que el puerto 80 esté expuesto: `docker exec minimarket-web netstat -tlnp | grep 80`

### Problema: Frontend no carga

**Causa**: Traefik no puede enrutar al servicio

**Solución**:
1. Verifica que `expose: - "80"` esté configurado (no `ports:`)
2. Verifica que las labels de Traefik estén correctas
3. Verifica que el servicio `web` esté en la misma red que Traefik
4. Verifica logs de Traefik para errores de enrutamiento

### Problema: API no responde

**Causa**: Nginx no puede hacer proxy al backend

**Solución**:
1. Verifica que `nginx.conf` tenga `proxy_pass http://api:5000;`
2. Verifica que el servicio `api` esté corriendo: `docker logs minimarket-api`
3. Verifica que ambos servicios estén en la misma red Docker
4. Prueba desde dentro del contenedor: `docker exec minimarket-web wget -O- http://api:5000/health`

---

## 📊 Arquitectura Final

```
Internet
   ↓
DNS: minimarket.edvio.app → 103.138.188.233
   ↓
Traefik (Coolify) - Puerto 80/443
   ↓
Labels Traefik detectan: minimarket.edvio.app
   ↓
Servicio web:80 (Nginx + Angular)
   ↓
   ├─ / → index.html (Angular SPA)
   └─ /api/* → api:5000 (.NET Core API)
                  ↓
              db:1433 (SQL Server)
```

---

## 🎯 Checklist Final

Antes de desplegar, verifica:

- [ ] DNS configurado: `minimarket.edvio.app` → `103.138.188.233`
- [ ] `coolify.yml` usa `letsencrypt` (no `myresolver`)
- [ ] `coolify.yml` usa `minimarket` en routers (no `web`)
- [ ] `coolify.yml` incluye servicio `db`
- [ ] Variables de entorno configuradas en Coolify
- [ ] Google OAuth configurado con `https://minimarket.edvio.app`
- [ ] Health checks configurados correctamente
- [ ] `expose:` usado en lugar de `ports:` para servicio `web`

---

## 📝 Archivos Modificados

- ✅ `coolify.yml` - Configuración completa con todos los servicios
- ✅ `docker-compose.yml` - Configuración para desarrollo local
- ✅ Labels de Traefik corregidos
- ✅ Health checks optimizados
- ✅ Connection string configurado correctamente

---

## 🚀 Resultado Esperado

Después de aplicar esta configuración:

- ✅ `https://minimarket.edvio.app` carga el frontend
- ✅ `https://minimarket.edvio.app/api/health` responde correctamente
- ✅ Certificado SSL válido de Let's Encrypt
- ✅ Sin errores de certificado en Chrome
- ✅ Redirección HTTP → HTTPS automática
- ✅ Todos los servicios en estado "Healthy"
- ✅ Traefik genera certificados automáticamente

**¡Listo para producción!** 🎉

