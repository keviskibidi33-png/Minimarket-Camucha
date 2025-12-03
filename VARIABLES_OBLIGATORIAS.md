# ⚠️ VARIABLES OBLIGATORIAS para Coolify

## 🔴 CRÍTICO: Estas variables DEBEN estar configuradas en Coolify

Si falta alguna de estas variables, el contenedor se apagará inmediatamente (Exited).

### 1. `DB_PASSWORD` ⚠️ **MÁS IMPORTANTE**

**¿Qué pasa si falta?**
- SQL Server se apaga automáticamente por seguridad
- El contenedor `db` mostrará estado: 🔴 **Exited**
- El contenedor `app` no podrá conectarse y también fallará

**Requisitos de la contraseña:**
- ✅ Mínimo 8 caracteres
- ✅ Debe incluir mayúsculas (A-Z)
- ✅ Debe incluir minúsculas (a-z)
- ✅ Debe incluir números (0-9)
- ✅ Debe incluir caracteres especiales (!@#$%^&*)

**Ejemplos válidos:**
- ✅ `MyStr0ng!P@ssw0rd`
- ✅ `Minimarket2024!Secure`
- ✅ `DB_P@ss123!Strong`

**Ejemplos INVÁLIDOS (causarán fallo):**
- ❌ `password` (muy débil, solo minúsculas)
- ❌ `12345678` (solo números)
- ❌ `Password` (falta número y especial)
- ❌ `pass123` (muy corta, falta especial)

### 2. `JWT_SECRET_KEY` ⚠️ **OBLIGATORIA**

**¿Qué pasa si falta?**
- La aplicación .NET no podrá generar tokens JWT
- El contenedor `app` puede iniciar pero fallará al autenticar usuarios

**Requisitos:**
- ✅ Mínimo 32 caracteres (recomendado 64+)
- ✅ Puede ser cualquier texto largo y aleatorio

**Generar una clave segura:**
```bash
openssl rand -base64 64
```

**Ejemplo válido:**
```
JWT_SECRET_KEY=SuperSecretKeyForJWT_MinimumLengthIs32Characters_UseLongRandomString123456789
```

### 3. `BASE_URL` ⚠️ **OBLIGATORIA**

**¿Qué pasa si falta?**
- La aplicación funcionará pero las URLs generadas serán incorrectas
- Los emails y redirecciones no funcionarán correctamente

**Ejemplo para producción:**
```
BASE_URL=https://api-minimarket.edvio.app
```

### 4. `FRONTEND_URL` ⚠️ **OBLIGATORIA**

**Ejemplo para producción:**
```
FRONTEND_URL=https://minimarket.edvio.app
```

### 5. `CORS_ORIGINS` ⚠️ **OBLIGATORIA**

**Ejemplo para producción:**
```
CORS_ORIGINS=https://minimarket.edvio.app
```

### 6. `GOOGLE_REDIRECT_URI` ⚠️ **OBLIGATORIA** (si usas Google OAuth)

**Ejemplo para producción:**
```
GOOGLE_REDIRECT_URI=https://api-minimarket.edvio.app/api/auth/google-callback
```

---

## 📋 Checklist Rápido para Coolify

Antes de hacer Deploy, verifica que tienes estas 6 variables:

- [ ] `DB_PASSWORD` = Contraseña fuerte (8+ caracteres, mayúsculas, minúsculas, números, especiales)
- [ ] `JWT_SECRET_KEY` = Clave larga (mínimo 32 caracteres)
- [ ] `BASE_URL` = https://api-minimarket.edvio.app
- [ ] `FRONTEND_URL` = https://minimarket.edvio.app
- [ ] `CORS_ORIGINS` = https://minimarket.edvio.app
- [ ] `GOOGLE_REDIRECT_URI` = https://api-minimarket.edvio.app/api/auth/google-callback

---

## 🔍 Cómo verificar si falta una variable

1. Ve a Coolify → Tu Proyecto → **Environment Variables**
2. Busca cada una de las variables de arriba
3. Si alguna está vacía o no existe, **agrégala**
4. Guarda y haz **Redeploy**

---

## 🐛 Si el contenedor sigue en 🔴 Exited

### Paso 1: Ver los Logs
1. En Coolify, haz clic en el contenedor que está en rojo
2. Ve a la pestaña **Logs**
3. Busca estos mensajes de error:

**Si ves:**
- `Login failed for user 'sa'` → La contraseña `DB_PASSWORD` es incorrecta o muy débil
- `Password validation failed` → La contraseña `DB_PASSWORD` no cumple los requisitos
- `OOM Killed` o `Memory limit` → Tu servidor se quedó sin RAM (necesitas más memoria)
- `Variable ${DB_PASSWORD} is not set` → Falta configurar `DB_PASSWORD` en Coolify

### Paso 2: Verificar Memoria RAM
SQL Server necesita al menos **2GB de RAM libre**. Si tu servidor tiene menos, considera:
- Cerrar otras aplicaciones
- Aumentar la RAM del servidor
- Usar una versión más ligera de SQL Server

---

## ✅ Variables Opcionales (tienen valores por defecto)

Estas variables **NO son obligatorias** porque ya tienen valores por defecto:

- `DB_NAME` (por defecto: `MinimarketDB`)
- `DB_USER` (por defecto: `SA`)
- `SMTP_SERVER` (por defecto: `smtp.gmail.com`)
- `SMTP_USER` (por defecto: `minimarket.camucha@gmail.com`)
- `SMTP_PASSWORD` (por defecto: ya configurado)
- `GOOGLE_CLIENT_ID` (por defecto: ya configurado)
- `GOOGLE_CLIENT_SECRET` (por defecto: ya configurado)

Solo configúralas si quieres cambiar los valores por defecto.

