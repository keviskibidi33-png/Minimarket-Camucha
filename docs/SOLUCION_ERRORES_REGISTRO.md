# Solución de Errores en Registro de Usuarios

## 🔴 Error 1: Google OAuth 403 - "The given origin is not allowed for the given client ID"

### Problema
```
Failed to load resource: the server responded with a status of 403
[GSI_LOGGER]: The given origin is not allowed for the given client ID
```

### Causa
La URL de origen (`http://localhost:4200`) no está configurada en Google Cloud Console como un origen autorizado para el Client ID de OAuth.

### Solución

1. **Ir a Google Cloud Console**
   - URL: https://console.cloud.google.com/
   - Seleccionar el proyecto correspondiente

2. **Navegar a Credenciales**
   - APIs & Services → Credentials
   - Buscar el OAuth 2.0 Client ID: `259590059487-5k8bk2sor6r9oa02pojkhj5nrd8c9h2e.apps.googleusercontent.com`

3. **Editar el Client ID**
   - Click en el nombre del Client ID para editarlo

4. **Agregar Orígenes Autorizados**
   - En "Authorized JavaScript origins", agregar:
     - `http://localhost:4200`
     - `https://localhost:4200` (si usas HTTPS)
     - `http://localhost:5000` (para el backend si es necesario)
   
5. **Agregar URIs de Redirección**
   - En "Authorized redirect URIs", agregar:
     - `http://localhost:5000/api/auth/google-callback`
     - `http://localhost:4200/auth/google-callback` (si el frontend maneja el callback)

6. **Guardar cambios**
   - Click en "Save"
   - Esperar 1-2 minutos para que los cambios se propaguen

### Verificación
- Recargar la página de registro
- El botón de Google Sign-In debería funcionar sin error 403

---

## 🔴 Error 2: 500 Internal Server Error en /api/auth/register

### Problema
```
Failed to load resource: the server responded with a status of 500 (Internal Server Error)
POST http://localhost:5000/api/auth/register
Error: "One or more validation failures have occurred."
```

### ✅ SOLUCIÓN IMPLEMENTADA

**Causa**: El `try-catch` en `AuthController.Register()` estaba capturando la `ValidationException` de FluentValidation y devolviendo un 500 en lugar de dejar que el `GlobalExceptionHandlerMiddleware` la maneje correctamente como 400.

**Fix aplicado**: 
- Removido el `try-catch` del método `Register()`
- Ahora el middleware captura `ValidationException` y devuelve **400 BadRequest** con los errores de validación detallados
- Los errores de validación ahora se muestran correctamente al usuario

### Posibles Causas Adicionales (si el error persiste)

#### 1. **Error en la Base de Datos**
- Conexión a la base de datos fallida
- Tabla `UserProfiles` no existe
- Constraint violation (DNI duplicado, etc.)

#### 2. **Error en el Envío de Email**
- Configuración de SMTP incorrecta
- Credenciales de email inválidas
- Servidor SMTP no disponible
- **Nota**: El envío de email es asíncrono y no debería causar error 500

#### 3. **Error en la Creación del Usuario**
- Validación de contraseña fallida
- Usuario ya existe
- Error al asignar rol

### Solución Paso a Paso

#### Paso 1: Revisar Logs del Backend
```bash
# Ver logs en tiempo real
cd src/Minimarket.API
dotnet run
```

Buscar en la consola el error específico que está causando el 500.

#### Paso 2: Verificar Base de Datos
```sql
-- Verificar que la tabla UserProfiles existe
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'UserProfiles';

-- Verificar estructura
SELECT * FROM UserProfiles;
```

#### Paso 3: Verificar Configuración de Email
Revisar `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "minimarket.camucha@gmail.com",
    "SmtpPassword": "hwob kkow ikoi ybnx",
    "FromEmail": "minimarket.camucha@gmail.com"
  }
}
```

**Nota**: El envío de email es asíncrono y no debería causar error 500. Si falla, solo se loguea un warning.

#### Paso 4: Probar Registro desde Swagger
1. Ir a: http://localhost:5000/swagger
2. Expandir `POST /api/auth/register`
3. Probar con datos de ejemplo:
```json
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test123!",
  "firstName": "Test",
  "lastName": "User",
  "phone": "999999999",
  "dni": "12345678"
}
```

#### Paso 5: Revisar Validaciones
El `RegisterCommandHandler` valida:
- Email único
- Username único
- DNI único (si se proporciona)
- Contraseña válida (requisitos de Identity)

### Soluciones Comunes

#### Si el error es "Email ya existe":
- Usar un email diferente
- O eliminar el usuario existente de la base de datos

#### Si el error es "DNI ya existe":
- Usar un DNI diferente
- O eliminar el perfil existente

#### Si el error es de contraseña:
- La contraseña debe cumplir los requisitos de Identity:
  - Mínimo 6 caracteres
  - Al menos una mayúscula
  - Al menos una minúscula
  - Al menos un número
  - Al menos un carácter especial

#### Si el error es de base de datos:
```bash
# Ejecutar migraciones
cd src/Minimarket.API
dotnet ef database update --project ../Minimarket.Infrastructure
```

### Manejo de Errores Mejorado

**Implementado**: 
- El `GlobalExceptionHandlerMiddleware` ahora maneja correctamente las `ValidationException`
- Las validaciones de FluentValidation devuelven **400 BadRequest** con lista de errores
- El formato de respuesta es consistente con el resto de la API

**Estructura de respuesta para errores de validación**:
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    "El correo electrónico es requerido",
    "El nombre de usuario debe tener al menos 3 caracteres"
  ],
  "traceId": "...",
  "correlationId": "...",
  "timestamp": "2025-01-XX..."
}
```

---

## 🔍 Debugging Adicional

### Habilitar Logging Detallado

En `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "Minimarket": "Debug"
    }
  }
}
```

### Verificar que el Seeder se Ejecutó

El seeder crea el rol "Cliente" automáticamente. Si no existe, puede causar errores.

Verificar en la base de datos:
```sql
SELECT * FROM AspNetRoles WHERE Name = 'Cliente';
```

Si no existe, ejecutar el seeder manualmente o reiniciar la aplicación.

---

## ✅ Checklist de Verificación

- [ ] Google OAuth: Origen `http://localhost:4200` agregado en Google Cloud Console
- [ ] Base de datos: Migraciones ejecutadas correctamente
- [ ] Base de datos: Tabla `UserProfiles` existe
- [ ] Base de datos: Rol "Cliente" existe
- [ ] Backend: Logs muestran el error específico
- [ ] Email: Configuración SMTP correcta (opcional, no bloquea registro)
- [ ] Validación: Contraseña cumple requisitos
- [ ] Validación: Email y username únicos

---

**Última actualización**: 2025-01-XX

