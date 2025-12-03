# 🔐 Configuración de Google Sign-In para Producción

## 📋 Orígenes que debes agregar en Google Cloud Console

### Paso 1: Acceder a Google Cloud Console

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona tu proyecto
3. Navega a: **APIs & Services** → **Credentials**
4. Busca y edita el OAuth 2.0 Client ID: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e`

### Paso 2: Agregar Orígenes JavaScript Autorizados

En la sección **"Authorized JavaScript origins"**, agrega estos orígenes (uno por línea):

```
https://minimarket.edvio.app
http://localhost:4200
https://localhost:4200
```

### Paso 3: Agregar URIs de Redirección Autorizados

En la sección **"Authorized redirect URIs"**, agrega estas URIs (una por línea):

```
https://api-minimarket.edvio.app/api/auth/google-callback
http://localhost:5000/api/auth/google-callback
https://localhost:5000/api/auth/google-callback
```

### Paso 4: Guardar

Haz clic en **"Save"** y espera unos minutos para que los cambios se propaguen.

---

## ✅ Verificación

Después de guardar, espera 5-10 minutos y luego:

1. Abre `https://minimarket.edvio.app` en el navegador
2. Abre la consola del navegador (F12)
3. Intenta iniciar sesión con Google
4. No deberías ver el error: `[GSI_LOGGER]: The given origin is not allowed for the given client ID`

---

## 📝 Notas Importantes

- Los cambios en Google Cloud Console pueden tardar hasta 10 minutos en propagarse
- Asegúrate de que los orígenes coincidan **exactamente** (incluyendo `https://` vs `http://`)
- No agregues barras finales (`/`) a los orígenes
- El Client ID debe ser: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e.apps.googleusercontent.com`

