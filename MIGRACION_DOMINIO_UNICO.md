# 🔄 Guía de Migración a Dominio Único

## 📋 Resumen

Esta guía explica cómo migrar de una configuración con dominios separados (frontend y backend) a un **dominio único** donde todo se sirve desde el mismo dominio.

## ✅ Cambios Realizados en el Código

### 1. Eliminación de URLs Hardcodeadas

Se eliminaron las URLs hardcodeadas en `Program.cs`:
- ❌ Antes: `https://minimarket.edvio.app` y `https://api-minimarket.edvio.app` estaban hardcodeadas
- ✅ Ahora: Solo se usan las URLs configuradas en variables de entorno

**Archivo modificado:** `src/Minimarket.API/Program.cs`

## 🔧 Configuración Requerida en Coolify

### Variables de Entorno a Actualizar

Cuando migres a un dominio único (por ejemplo: `https://minimarket.edvio.app`), configura estas variables en Coolify:

```bash
# URL base de la API (mismo dominio que el frontend)
BASE_URL=https://minimarket.edvio.app

# URL del frontend (mismo dominio)
FRONTEND_URL=https://minimarket.edvio.app

# Orígenes permitidos para CORS (solo el dominio único)
CORS_ORIGINS=https://minimarket.edvio.app

# Redirect URI de Google OAuth (mismo dominio)
GOOGLE_REDIRECT_URI=https://minimarket.edvio.app/api/auth/google-callback
```

### ⚠️ IMPORTANTE

1. **Reemplaza `minimarket.edvio.app`** con tu dominio real si es diferente
2. **No incluyas** `/api` al final de `BASE_URL` ni `FRONTEND_URL`
3. **No incluyas** barras finales (`/`) en ninguna URL

## 🔐 Configuración de Google OAuth

### Paso 1: Actualizar Google Cloud Console

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona tu proyecto
3. Ve a **APIs & Services** > **Credentials**
4. Edita el OAuth 2.0 Client ID: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e`

### Paso 2: Actualizar Authorized JavaScript origins

En la sección **"Authorized JavaScript origins"**, asegúrate de tener:

```
https://minimarket.edvio.app
http://localhost:4200
https://localhost:4200
```

**Nota:** Si ya no usas `api-minimarket.edvio.app`, puedes eliminarlo de aquí.

### Paso 3: Actualizar Authorized redirect URIs

En la sección **"Authorized redirect URIs"**, actualiza a:

```
https://minimarket.edvio.app/api/auth/google-callback
http://localhost:5000/api/auth/google-callback
https://localhost:5000/api/auth/google-callback
```

**Nota:** Si ya no usas `api-minimarket.edvio.app`, elimina su redirect URI de aquí.

### Paso 4: Guardar y Esperar

1. Haz clic en **"Save"**
2. Espera 5-10 minutos para que los cambios se propaguen

## ✅ Ventajas de Usar un Dominio Único

1. **Simplifica CORS**: No necesitas configurar CORS complejo porque no hay cross-origin
2. **Mejor rendimiento**: Menos latencia al no hacer requests entre dominios
3. **Cookies más simples**: Las cookies funcionan automáticamente sin configuración especial
4. **Menos configuración**: Menos variables de entorno que mantener

## 🧪 Verificación Post-Migración

Después de aplicar los cambios, verifica:

1. **Frontend carga correctamente:**
   ```bash
   curl -I https://minimarket.edvio.app
   ```

2. **API responde correctamente:**
   ```bash
   curl https://minimarket.edvio.app/api/health
   ```

3. **No hay errores de CORS:**
   - Abre `https://minimarket.edvio.app` en el navegador
   - Abre la consola del navegador (F12)
   - Verifica que no haya errores de CORS

4. **Google Sign-In funciona:**
   - Intenta iniciar sesión con Google
   - No deberías ver errores 403

## 📝 Notas Importantes

- El frontend ya está configurado para usar URLs relativas (`/api`), así que funcionará automáticamente con el dominio único
- Los cambios en Google Cloud Console pueden tardar hasta 10 minutos en propagarse
- Asegúrate de reiniciar la aplicación en Coolify después de cambiar las variables de entorno

## 🔄 Rollback (Si algo sale mal)

Si necesitas volver a la configuración anterior:

1. Restaura las variables de entorno en Coolify:
   ```bash
   BASE_URL=https://api-minimarket.edvio.app
   FRONTEND_URL=https://minimarket.edvio.app
   CORS_ORIGINS=https://minimarket.edvio.app
   GOOGLE_REDIRECT_URI=https://api-minimarket.edvio.app/api/auth/google-callback
   ```

2. Actualiza Google Cloud Console con las URLs anteriores

3. Reinicia la aplicación en Coolify

