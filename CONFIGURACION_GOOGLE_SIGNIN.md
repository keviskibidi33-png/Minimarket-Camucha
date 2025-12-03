# Configuración de Google Sign-In en Google Cloud Console

## Problema
El error `403` y `[GSI_LOGGER]: The given origin is not allowed for the given client ID` indica que el origen `https://minimarket.edvio.app` no está autorizado en Google Cloud Console.

## Solución

### 1. Acceder a Google Cloud Console
1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona tu proyecto (o crea uno nuevo si no tienes)
3. Ve a **APIs & Services** > **Credentials**

### 2. Encontrar o crear OAuth 2.0 Client ID
1. Busca el Client ID: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e.apps.googleusercontent.com`
2. Si no existe, crea uno nuevo:
   - Click en **+ CREATE CREDENTIALS** > **OAuth client ID**
   - Application type: **Web application**
   - Name: `Minimarket Camucha Web Client`

### 3. Configurar Authorized JavaScript origins
En la sección **Authorized JavaScript origins**, agrega:

```
https://minimarket.edvio.app
http://localhost:4200
https://localhost:4200
```

**Importante:** 
- No incluyas la barra final (`/`)
- Usa `https://` para producción
- Usa `http://` y `https://` para desarrollo local

### 4. Configurar Authorized redirect URIs
En la sección **Authorized redirect URIs**, agrega:

```
https://api-minimarket.edvio.app/api/auth/google-callback
http://localhost:5000/api/auth/google-callback
https://localhost:5000/api/auth/google-callback
```

**Nota:** El endpoint `/api/auth/google-signin` NO necesita estar en redirect URIs porque es un endpoint POST que recibe el token directamente del frontend.

### 5. Guardar cambios
1. Click en **SAVE**
2. Espera unos minutos para que los cambios se propaguen (puede tardar hasta 5 minutos)

### 6. Verificar configuración en el código

#### Frontend (`minimarket-web/src/environments/environment.prod.ts`)
```typescript
export const environment = {
  production: true,
  apiUrl: '/api',
  googleClientId: '259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e.apps.googleusercontent.com'
};
```

#### Backend (`env.production.example`)
```
GOOGLE_CLIENT_ID=259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=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
GOOGLE_REDIRECT_URI=https://api-minimarket.edvio.app/api/auth/google-callback
FRONTEND_URL=https://minimarket.edvio.app
```

### 7. Probar
1. Abre `https://minimarket.edvio.app/auth/login`
2. Click en el botón "Acceder con Google"
3. Deberías ver el popup de Google sin errores 403

## Resumen de URLs a configurar

### Authorized JavaScript origins:
- `https://minimarket.edvio.app`
- `http://localhost:4200`
- `https://localhost:4200`

### Authorized redirect URIs:
- `https://api-minimarket.edvio.app/api/auth/google-callback`
- `http://localhost:5000/api/auth/google-callback`
- `https://localhost:5000/api/auth/google-callback`

## Troubleshooting

### Error 403 persiste después de configurar
- Espera 5-10 minutos para que los cambios se propaguen
- Limpia la caché del navegador (Ctrl+Shift+Delete)
- Verifica que el Client ID en el código coincida exactamente con el de Google Cloud Console

### Error "redirect_uri_mismatch"
- Verifica que la URL en `GOOGLE_REDIRECT_URI` coincida exactamente con la configurada en Google Cloud Console
- No incluyas barras finales ni parámetros adicionales

### El botón de Google no aparece
- Verifica que el script de Google Identity Services esté cargado en `index.html`
- Revisa la consola del navegador para errores de JavaScript
- Verifica que `environment.googleClientId` esté configurado correctamente

