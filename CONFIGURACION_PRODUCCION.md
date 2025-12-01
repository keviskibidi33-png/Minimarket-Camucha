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

### Paso 2: Configurar variables obligatorias

Agrega estas variables en Coolify (las demás ya tienen valores por defecto):

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `DB_CONNECTION_STRING` | Connection string de SQL Server | `Server=...;Database=...;...` |
| `JWT_SECRET_KEY` | Clave secreta para JWT (mínimo 32 caracteres) | Genera con: `openssl rand -base64 64` |
| `BASE_URL` | URL base de tu API | `https://api.tudominio.com` |
| `FRONTEND_URL` | URL de tu frontend | `https://tudominio.com` |
| `CORS_ORIGINS` | URLs permitidas (separadas por coma) | `https://tudominio.com,https://www.tudominio.com` |

### Paso 3: Variables opcionales (ya configuradas por defecto)

Estas variables **ya tienen valores por defecto** del sistema. Solo configúralas si necesitas cambiarlas:

| Variable | Valor por Defecto | ¿Cuándo cambiar? |
|----------|-------------------|------------------|
| `SMTP_SERVER` | `smtp.gmail.com` | Si usas otro servidor SMTP |
| `SMTP_USER` | `minimarket.camucha@gmail.com` | Si cambias el email |
| `SMTP_PASSWORD` | `xzloatedigfqgyxi` | Si cambias la contraseña |
| `GOOGLE_CLIENT_ID` | `259590059487-...` | Si usas otras credenciales OAuth |
| `GOOGLE_CLIENT_SECRET` | `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` | Si usas otras credenciales OAuth |

### Paso 4: Google OAuth Redirect URI

**IMPORTANTE:** Actualiza el Redirect URI en Google Cloud Console:

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona tu proyecto
3. Ve a **APIs & Services > Credentials**
4. Edita tu OAuth 2.0 Client ID
5. Agrega el Redirect URI: `https://tu-api.com/api/auth/google-callback`
6. En Coolify, configura: `GOOGLE_REDIRECT_URI=https://tu-api.com/api/auth/google-callback`

---

## 📝 Variables con Valores por Defecto

El sistema ya tiene configuradas estas variables, **no necesitas configurarlas** a menos que quieras cambiarlas:

### Email SMTP (Gmail)
- ✅ `SMTP_SERVER=smtp.gmail.com`
- ✅ `SMTP_PORT=587`
- ✅ `SMTP_USER=minimarket.camucha@gmail.com`
- ✅ `SMTP_PASSWORD=xzloatedigfqgyxi`
- ✅ `FROM_EMAIL=minimarket.camucha@gmail.com`
- ✅ `FROM_NAME=Minimarket Camucha`

### Google OAuth
- ✅ `GOOGLE_CLIENT_ID=259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e.apps.googleusercontent.com`
- ✅ `GOOGLE_CLIENT_SECRET=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`

### Resend API
- El sistema usa Resend automáticamente como fallback si SMTP falla
- Solo configura `RESEND_API_KEY` si quieres usarlo como método principal

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
- [ ] Actualizar Redirect URI en Google Cloud Console
- [ ] Configurar `GOOGLE_REDIRECT_URI` en Coolify (si aplica)
- [ ] Verificar que las variables de Email y Google OAuth funcionen (ya están configuradas por defecto)

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

