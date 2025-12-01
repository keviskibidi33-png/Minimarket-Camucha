# Configuración para Producción

## 📋 Resumen

Este documento explica cómo configurar las variables de entorno para producción, tanto para desarrollo local con Docker Compose como para despliegue en Coolify.

## 🔧 Desarrollo Local (Docker Compose)

### Paso 1: Crear archivo .env

Copia el archivo de ejemplo:
```bash
cp .env.example .env
```

### Paso 2: Configurar variables obligatorias

Edita el archivo `.env` y configura **solo** las variables obligatorias:

```bash
# OBLIGATORIAS - Debes configurarlas
DB_CONNECTION_STRING=Server=tu-servidor;Database=MinimarketDB;...
JWT_SECRET_KEY=tu-clave-secreta-minimo-32-caracteres
BASE_URL=https://api.tudominio.com
FRONTEND_URL=https://tudominio.com
CORS_ORIGINS=https://tudominio.com,https://www.tudominio.com
```

**Nota:** Las demás variables (Email SMTP, Google OAuth) ya están configuradas por defecto en el sistema.

### Paso 3: Levantar contenedores

```bash
docker-compose up -d --build
```

---

## 🚀 Producción en Coolify

### ⚠️ IMPORTANTE: En Coolify NO uses archivo .env

En Coolify, las variables de entorno se configuran directamente en la **interfaz web**, no mediante archivo `.env`.

### Paso 1: Acceder a Coolify

1. Ve a tu instancia de Coolify
2. Selecciona tu aplicación
3. Ve a la sección **"Environment Variables"** o **"Variables de Entorno"**

### Paso 2: Configurar TODAS las variables en Coolify

**⚠️ IMPORTANTE:** En producción, TODAS las variables deben configurarse explícitamente. No hay valores por defecto por seguridad.

#### Variables Obligatorias:

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `DB_CONNECTION_STRING` | Connection string de SQL Server | `Server=...;Database=...;...` |
| `JWT_SECRET_KEY` | Clave secreta para JWT (mínimo 32 caracteres) | Genera con: `openssl rand -base64 64` |
| `BASE_URL` | URL base de tu API | `https://api.tudominio.com` |
| `FRONTEND_URL` | URL de tu frontend | `https://tudominio.com` |
| `CORS_ORIGINS` | URLs permitidas (separadas por coma) | `https://tudominio.com,https://www.tudominio.com` |

#### Variables de Email (Obligatorias):

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `SMTP_SERVER` | Servidor SMTP | `smtp.gmail.com` |
| `SMTP_PORT` | Puerto SMTP | `587` |
| `SMTP_USER` | Usuario SMTP | `tu-email@gmail.com` |
| `SMTP_PASSWORD` | Contraseña SMTP o App Password | `tu-contraseña` |
| `FROM_EMAIL` | Email remitente | `tu-email@gmail.com` |
| `FROM_NAME` | Nombre remitente | `Minimarket Camucha` |

#### Variables de Google OAuth (Obligatorias):

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `GOOGLE_CLIENT_ID` | Client ID de Google OAuth | `xxx.apps.googleusercontent.com` |
| `GOOGLE_CLIENT_SECRET` | Client Secret de Google OAuth | `GOCSPX-xxx` |
| `GOOGLE_REDIRECT_URI` | Redirect URI completo | `https://tu-api.com/api/auth/google-callback` |

### Paso 4: Google OAuth Redirect URI

**IMPORTANTE:** Actualiza el Redirect URI en Google Cloud Console:

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona tu proyecto
3. Ve a **APIs & Services > Credentials**
4. Edita tu OAuth 2.0 Client ID
5. Agrega el Redirect URI: `https://tu-api.com/api/auth/google-callback`
6. En Coolify, configura: `GOOGLE_REDIRECT_URI=https://tu-api.com/api/auth/google-callback`

---

## 📝 Nota sobre Valores por Defecto

**⚠️ IMPORTANTE:** En producción, NO hay valores por defecto por seguridad. Todas las variables deben configurarse explícitamente en Coolify.

Los valores que ves en `appsettings.json` son **solo para desarrollo local**. En producción, todas las claves y credenciales deben configurarse a través de variables de entorno en Coolify.

### Resend API (Opcional)
- El sistema puede usar Resend automáticamente como fallback si SMTP falla
- Solo configura `RESEND_API_KEY` si quieres usarlo como método principal o fallback

---

## 🔐 Seguridad

### ✅ Archivo .env está en .gitignore

El archivo `.env` **NO se sube a Git** (está en `.gitignore`). Solo el archivo `.env.example` está en el repositorio como referencia.

### ✅ Variables sensibles en Coolify

En Coolify, las variables se almacenan de forma segura y encriptada. No las compartas públicamente.

---

## 📋 Checklist para Producción

- [ ] Configurar `DB_CONNECTION_STRING` en Coolify
- [ ] Generar y configurar `JWT_SECRET_KEY` (mínimo 32 caracteres)
- [ ] Configurar `BASE_URL` con tu dominio de producción
- [ ] Configurar `FRONTEND_URL` con tu dominio de producción
- [ ] Configurar `CORS_ORIGINS` con tus dominios permitidos
- [ ] Configurar todas las variables de Email SMTP (`SMTP_SERVER`, `SMTP_USER`, `SMTP_PASSWORD`, etc.)
- [ ] Configurar todas las variables de Google OAuth (`GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `GOOGLE_REDIRECT_URI`)
- [ ] Actualizar Redirect URI en Google Cloud Console para que coincida con `GOOGLE_REDIRECT_URI`
- [ ] (Opcional) Configurar `RESEND_API_KEY` si quieres usar Resend como fallback

---

## 🆘 Troubleshooting

### El sistema no envía emails
- Verifica que `SMTP_PASSWORD` sea correcta
- Verifica que el email tenga "Acceso de aplicaciones menos seguras" habilitado (Gmail)
- El sistema intentará usar Resend API automáticamente si SMTP falla

### Error de CORS
- Verifica que `CORS_ORIGINS` incluya exactamente la URL de tu frontend
- Asegúrate de incluir `https://` o `http://` según corresponda

### Google OAuth no funciona
- Verifica que el Redirect URI en Google Cloud Console coincida con `GOOGLE_REDIRECT_URI`
- El formato debe ser: `https://tu-api.com/api/auth/google-callback`

