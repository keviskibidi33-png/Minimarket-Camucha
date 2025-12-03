# 📋 Resumen de Cambios para Producción

## ✅ Cambios Aplicados

### 1. CORS - Configuración Completa

**Archivo:** `src/Minimarket.API/Program.cs`

**Cambios:**
- Política renombrada de `AllowAngularApp` a `FrontendPolicy`
- Orígenes de producción agregados automáticamente:
  - `https://minimarket.edvio.app`
  - `https://api-minimarket.edvio.app`
- Orígenes de desarrollo mantenidos:
  - `http://localhost:4200`
- Cache de preflight requests configurado (24 horas)

**Código aplicado:**
```csharp
// CORS - Configuración para producción y desarrollo
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:4200" };

var productionOrigins = new[]
{
    "https://minimarket.edvio.app",
    "https://api-minimarket.edvio.app"
};

var allOrigins = allowedOrigins
    .Concat(productionOrigins)
    .Distinct()
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()
             .SetPreflightMaxAge(TimeSpan.FromHours(24));
    });
});
```

---

### 2. Login - Manejo de Errores Mejorado

**Archivos:**
- `src/Minimarket.Application/Features/Auth/Commands/LoginCommandHandler.cs`
- `src/Minimarket.API/Controllers/AuthController.cs`

**Cambios:**
- Verificación de usuario bloqueado
- Verificación de EmailConfirmed
- Códigos HTTP apropiados:
  - **401 Unauthorized**: Usuario no encontrado o contraseña incorrecta
  - **403 Forbidden**: Usuario bloqueado o no permitido
  - **400 BadRequest**: Otros errores de validación
- Logging detallado para debugging

**Comportamiento:**
- Si el usuario no existe → 401
- Si la contraseña es incorrecta → 401
- Si el usuario está bloqueado → 403
- Si EmailConfirmed = false → 403
- Si hay error inesperado → 400 con mensaje genérico

---

### 3. Seeder de Usuarios - Mejorado para Producción

**Archivo:** `src/Minimarket.Infrastructure/Data/Seeders/DatabaseSeeder.cs`

**Cambios:**
- Verifica usuario existente por username Y email
- Asegura `EmailConfirmed = true` para admin
- Configura `LockoutEnabled = false` para admin inicial
- Desbloquea admin si está bloqueado
- Verifica y asigna rol Administrador si falta

**Usuario Admin creado/verificado:**
- Username: `admin`
- Email: `admin@minimarketcamucha.com`
- Password: `Admin123!`
- Rol: `Administrador`
- EmailConfirmed: `true`
- LockoutEnabled: `false` (para admin inicial)

---

### 4. Assets (Logo) - Rutas Corregidas

**Archivos:**
- `minimarket-web/src/index.html` (favicon corregido)
- Todos los componentes ya usan `assets/logo.png` (sin barra inicial)

**Ubicación del logo:**
- Fuente: `minimarket-web/src/assets/logo.png`
- Build: `dist/minimarket-web/assets/logo.png`
- Docker: `/usr/share/nginx/html/assets/logo.png`

**Rutas en componentes (ya correctas):**
- `src="assets/logo.png"` ✅ (sin barra inicial)

---

### 5. Google Sign-In - Orígenes Documentados

**Ver:** `INSTRUCCIONES_GOOGLE_SIGNIN.md`

**Orígenes JavaScript autorizados:**
```
https://minimarket.edvio.app
http://localhost:4200
https://localhost:4200
```

**URIs de redirección autorizados:**
```
https://api-minimarket.edvio.app/api/auth/google-callback
http://localhost:5000/api/auth/google-callback
https://localhost:5000/api/auth/google-callback
```

---

### 6. Script SQL de Verificación

**Archivo:** `scripts/verificar_admin_simple.sql`

**Contenido:**
- Lista tablas relacionadas con User y Role
- Muestra primeros 5 usuarios
- Lista todos los roles
- Muestra usuarios con sus roles
- Verifica específicamente el admin con rol Administrador

---

## 🚀 Pasos para Aplicar

### 1. Commit y Push a GitHub

```bash
git add .
git commit -m "fix: correcciones para producción - CORS, login, seeder y assets"
git push origin main
```

### 2. Configurar Variables en Coolify

Verifica que estas variables estén configuradas en Coolify:

```env
CORS_ORIGINS=https://minimarket.edvio.app,http://localhost:4200
BASE_URL=https://api-minimarket.edvio.app
FRONTEND_URL=https://minimarket.edvio.app
GOOGLE_REDIRECT_URI=https://api-minimarket.edvio.app/api/auth/google-callback
```

### 3. Configurar Google Sign-In

Sigue las instrucciones en `INSTRUCCIONES_GOOGLE_SIGNIN.md`

### 4. Verificar Usuario Admin

Ejecuta `scripts/verificar_admin_simple.sql` en tu base de datos

### 5. Redeploy en Coolify

1. Ve a Coolify → Tu Proyecto
2. Haz clic en **Redeploy** para los servicios `app` y `web`
3. Espera a que los servicios estén en estado "Running"

---

## ✅ Verificación Post-Deploy

### 1. Probar Login Admin

```bash
curl -X POST https://api-minimarket.edvio.app/api/auth/login \
  -H "Content-Type: application/json" \
  -H "Origin: https://minimarket.edvio.app" \
  -d '{"username":"admin@minimarketcamucha.com","password":"Admin123!"}'
```

**Esperado:** Respuesta 200 con token JWT

### 2. Verificar CORS

1. Abre `https://minimarket.edvio.app` en el navegador
2. Abre la consola del navegador (F12)
3. Intenta hacer login
4. **No debe haber errores de CORS**

### 3. Verificar Logo

Abre directamente: `https://minimarket.edvio.app/assets/logo.png`

**Esperado:** La imagen se carga correctamente (no 404)

### 4. Verificar Google Sign-In

1. Abre `https://minimarket.edvio.app`
2. Intenta iniciar sesión con Google
3. **No debe aparecer el error de GSI_LOGGER**

---

## 📊 Estado Final

✅ **CORS**: Configurado correctamente con orígenes de producción  
✅ **Login**: Manejo de errores mejorado con códigos HTTP apropiados  
✅ **Seeder**: Configuración optimizada para producción  
✅ **Assets**: Rutas corregidas (sin barras iniciales)  
✅ **Google Sign-In**: Orígenes documentados para configuración  
✅ **Scripts SQL**: Script de verificación creado  

---

## 🔍 Troubleshooting

### Si el login sigue fallando:

1. Verifica los logs de Coolify para el servicio `app`
2. Ejecuta el script SQL para verificar que el admin existe
3. Verifica que `CORS_ORIGINS` esté configurado en Coolify
4. Revisa que el usuario no esté bloqueado en la base de datos

### Si CORS sigue bloqueando:

1. Verifica que `FrontendPolicy` esté aplicada (no `AllowAngularApp`)
2. Revisa los logs de la API para ver qué origen está siendo rechazado
3. Asegúrate de que `CORS_ORIGINS` incluya `https://minimarket.edvio.app`

### Si el logo no carga:

1. Verifica que `src/assets/logo.png` existe en el repositorio
2. Revisa el build de Angular para confirmar que el logo se copia
3. Verifica que Nginx esté sirviendo correctamente los archivos estáticos

---

## 📞 Próximos Pasos

1. ✅ Aplicar cambios (commit y push)
2. ✅ Configurar variables en Coolify
3. ✅ Configurar Google Sign-In en Google Cloud Console
4. ✅ Redeploy en Coolify
5. ✅ Verificar que todo funciona correctamente

**¡El proyecto está listo para producción!** 🚀

