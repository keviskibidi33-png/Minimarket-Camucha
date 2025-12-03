# 🔧 Correcciones para Producción - Minimarket Camucha

## 📋 Resumen de Problemas y Soluciones

### 1. ✅ CORS - Configuración Corregida

**Problema:** CORS bloqueando requests desde `https://minimarket.edvio.app`

**Solución:** Ver `Program.cs` actualizado con política `FrontendPolicy` que incluye:
- `https://minimarket.edvio.app` (producción)
- `http://localhost:4200` (desarrollo)
- `https://api-minimarket.edvio.app` (opcional, si es necesario)

---

### 2. ✅ Login - Manejo de Errores Mejorado

**Problema:** Login devuelve 400 genérico en lugar de códigos HTTP apropiados

**Solución:** `LoginCommandHandler` actualizado para:
- Devolver 401 (Unauthorized) para credenciales incorrectas
- Devolver 403 (Forbidden) para usuarios bloqueados
- Mejor logging de errores

---

### 3. ✅ Seeder de Usuarios - Configuración para Producción

**Problema:** Usuario admin puede tener configuración incorrecta (LockoutEnabled, etc.)

**Solución:** `DatabaseSeeder` mejorado para:
- Asegurar `EmailConfirmed = true`
- Configurar `LockoutEnabled = false` para admin inicial
- Verificar que el usuario existe antes de crear

---

### 4. ✅ Assets (Logo e Imágenes) - Configuración Completa

**Problema:** 404 en `/assets/logo.png` y logo no estaba en git

**Solución:** 
- ✅ Rutas ya corregidas a `assets/logo.png` (sin barra inicial)
- ✅ `angular.json` configurado correctamente con `"src/assets"`
- ✅ Logo agregado a git (forzado con `-f` porque estaba en `.gitignore`)
- ✅ `.gitignore` actualizado para permitir imágenes en `minimarket-web/src/assets/`
- ✅ Avatar de usuario corregido (ya no usa logo, usa iniciales o icono)

**Ubicación física del logo en producción:**
- En el build: `dist/minimarket-web/browser/assets/logo.png`
- En el contenedor Docker: `/usr/share/nginx/html/assets/logo.png`
- URL accesible: `https://minimarket.edvio.app/assets/logo.png`

**Imágenes agregadas a git:**
- ✅ `minimarket-web/src/assets/logo.png`
- ✅ `minimarket-web/src/assets/angelqr.jpg` (QR de pago)

**Ver documentación completa:** `ASSETS_CONFIGURACION.md`

---

### 5. ✅ Google Sign-In - Orígenes Autorizados

**Problema:** `[GSI_LOGGER]: The given origin is not allowed for the given client ID`

**Solución:** Agregar estos orígenes en Google Cloud Console:

#### Orígenes JavaScript autorizados:
```
https://minimarket.edvio.app
http://localhost:4200
https://localhost:4200
```

#### URI de redirección autorizados:
```
https://api-minimarket.edvio.app/api/auth/google-callback
http://localhost:5000/api/auth/google-callback
https://localhost:5000/api/auth/google-callback
```

**Instrucciones:**
1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona tu proyecto
3. APIs & Services → Credentials
4. Edita el OAuth 2.0 Client ID: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e`
5. Agrega los orígenes y URIs de redirección listados arriba
6. Guarda los cambios

---

## 📝 Archivos Modificados

### Backend (.NET)

1. **`src/Minimarket.API/Program.cs`**
   - ✅ CORS actualizado con política `FrontendPolicy`
   - ✅ Orígenes de producción y desarrollo configurados automáticamente
   - ✅ Cache de preflight requests configurado (24 horas)

2. **`src/Minimarket.Application/Features/Auth/Commands/LoginCommandHandler.cs`**
   - ✅ Manejo de errores mejorado con logging detallado
   - ✅ Verificación de usuario bloqueado
   - ✅ Verificación de EmailConfirmed
   - ✅ Manejo de lockout automático

3. **`src/Minimarket.API/Controllers/AuthController.cs`**
   - ✅ Códigos HTTP apropiados: 401 (Unauthorized), 403 (Forbidden), 400 (BadRequest)
   - ✅ Respuestas más descriptivas según el tipo de error

4. **`src/Minimarket.Infrastructure/Data/Seeders/DatabaseSeeder.cs`**
   - ✅ Configuración mejorada para producción
   - ✅ `LockoutEnabled = false` para admin inicial
   - ✅ Verificación de usuario existente antes de crear
   - ✅ Asegura EmailConfirmed = true
   - ✅ Desbloquea admin si está bloqueado

### Frontend (Angular)

5. **`minimarket-web/src/index.html`**
   - ✅ Ruta de favicon corregida (sin barra inicial)

### Scripts SQL

6. **`scripts/verificar_admin_simple.sql`**
   - ✅ Script simple para verificar usuarios y roles
   - ✅ Consultas directas sin lógica compleja

---

## 🚀 Pasos para Aplicar los Cambios

### 1. Actualizar CORS en Coolify

En Coolify, verifica que la variable de entorno `CORS_ORIGINS` esté configurada:

```env
CORS_ORIGINS=https://minimarket.edvio.app,http://localhost:4200
```

### 2. Verificar Usuario Admin

Ejecuta el script SQL `scripts/verificar_admin_simple.sql` en tu base de datos para verificar que el admin existe.

### 3. Configurar Google Sign-In

Agrega los orígenes listados arriba en Google Cloud Console.

### 4. Verificar Assets

Asegúrate de que `src/assets/logo.png` existe y se copia correctamente en el build.

### 5. Redeploy

Después de aplicar los cambios:
1. Haz commit y push a GitHub
2. En Coolify, haz Redeploy de los servicios `app` y `web`

---

## ✅ Checklist Final

- [ ] CORS configurado con orígenes correctos
- [ ] Login devuelve códigos HTTP apropiados
- [ ] Usuario admin existe con rol Administrador
- [ ] Google Sign-In configurado en Google Cloud Console
- [ ] Logo y assets cargando correctamente
- [ ] Variables de entorno en Coolify configuradas
- [ ] Redeploy completado

---

## 🔍 Verificación

### Probar Login:
```bash
curl -X POST https://api-minimarket.edvio.app/api/auth/login \
  -H "Content-Type: application/json" \
  -H "Origin: https://minimarket.edvio.app" \
  -d '{"username":"admin@minimarketcamucha.com","password":"Admin123!"}'
```

### Verificar CORS:
Abre la consola del navegador en `https://minimarket.edvio.app` y verifica que no hay errores de CORS.

### Verificar Logo:
Abre `https://minimarket.edvio.app/assets/logo.png` directamente en el navegador.

---

## 📞 Soporte

Si después de aplicar estos cambios sigues teniendo problemas:
1. Revisa los logs de Coolify para el servicio `app`
2. Revisa la consola del navegador para errores de CORS
3. Verifica que las variables de entorno estén correctamente configuradas

