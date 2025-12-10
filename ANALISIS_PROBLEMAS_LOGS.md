# Análisis y Validación de Problemas Identificados en los Logs

## Resumen Ejecutivo

Este documento valida los problemas reportados en los logs de la consola y proporciona análisis técnico detallado con soluciones específicas basadas en el código del proyecto.

---

## 🔍 Problema 1: Errores 401 Unauthorized

### Validación ✅

**Problema Reportado:**
- `/api/auth/profile:1 Failed to load resource: the server responded with a status of **401** ()`
- `/api/orders/my-orders:1 Failed to load resource: the server responded with a status of **401** ()`
- `/api/auth/addresses:1 Failed to load resource: the server responded with a status of **401** ()`

**Análisis del Código:**

1. **Interceptor de Autenticación** (`auth.interceptor.ts`):
   - ✅ El interceptor está correctamente configurado para agregar el token JWT en el header `Authorization: Bearer {token}`
   - ✅ Obtiene el token desde `localStorage.getItem('auth_token')` o desde `AuthService.getToken()`

2. **Manejo de Errores 401** (`error.interceptor.ts`):
   - ✅ El interceptor maneja errores 401 y redirige a `/login` cuando no es una ruta pública
   - ✅ Silencia errores 401 en endpoints opcionales (`/api/auth/profile`, `/api/auth/addresses`) cuando el usuario está en rutas públicas

3. **Configuración JWT** (`Program.cs`):
   - ✅ JWT está configurado con validación de Issuer, Audience, Lifetime y SigningKey
   - ⚠️ **PROBLEMA DETECTADO**: El token expira en 60 minutos por defecto (`ExpirationMinutes: 60`)

**Causas Probables:**

1. **Token Expirado**: El token JWT tiene una expiración de 60 minutos. Si el usuario permanece inactivo, el token expira.
2. **Token No Almacenado Correctamente**: El token podría no estar guardándose correctamente después del login.
3. **Token Inválido**: El token podría estar corrupto o no ser válido según los parámetros de validación del servidor.

**Soluciones Recomendadas:**

### Solución 1: Implementar Refresh Token (Recomendado)

```typescript
// En auth.service.ts - Agregar método para refrescar token
refreshToken(): Observable<LoginResponse> {
  return this.http.post<LoginResponse>(`${this.apiUrl}/refresh-token`, {
    token: this.getToken()
  }).pipe(
    tap(response => {
      this.storeAuth(response);
      this.isAuthenticated.set(true);
    })
  );
}
```

### Solución 2: Verificar Token Antes de Usar

```typescript
// En auth.interceptor.ts - Agregar validación de expiración
intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
  let token = this.authService.getToken();
  
  // Verificar si el token está expirado
  if (token && this.isTokenExpired(token)) {
    // Intentar refrescar el token
    this.authService.refreshToken().subscribe({
      next: (response) => {
        token = response.token;
      },
      error: () => {
        // Si falla el refresh, redirigir a login
        this.authService.logout();
      }
    });
  }
  
  // ... resto del código
}
```

### Solución 3: Aumentar Tiempo de Expiración (Temporal)

```json
// En appsettings.json o appsettings.Production.json
{
  "JwtSettings": {
    "ExpirationMinutes": 480  // 8 horas en lugar de 60 minutos
  }
}
```

---

## 🔍 Problema 2: Errores 500 Internal Server Error

### Validación ✅

**Problema Reportado:**
- `/api/sedes/b5d8af5a-0e2f-4b0d-b3ca-b58060ed4ca4:1 Failed to load resource: the server responded with a status of **500** ()`
- `/api/sedes:1 Failed to load resource: the server responded with a status of **500** ()` (Error al crear sede)
- `/api/ofertas:1 Failed to load resource: the server responded with a status of **500** ()` (Error creating oferta)

**Análisis del Código:**

1. **SedesController** (`SedesController.cs`):
   - ✅ Tiene manejo de excepciones con try-catch
   - ✅ Retorna 500 cuando hay excepciones no manejadas
   - ⚠️ **PROBLEMA POTENCIAL**: El error 500 puede ocurrir por:
     - Excepciones de base de datos (conexión, timeout, constraint violations)
     - Validaciones fallidas que no se capturan correctamente
     - Problemas con el UnitOfWork al guardar cambios

2. **OfertasController** (`OfertasController.cs`):
   - ✅ Similar estructura a SedesController
   - ✅ Maneja excepciones y retorna 500 para errores internos
   - ⚠️ **PROBLEMA POTENCIAL**: Validación de categorías/productos que no existen

3. **CreateSedeCommandHandler** (`CreateSedeCommandHandler.cs`):
   - ✅ Tiene logging detallado
   - ⚠️ **PROBLEMA DETECTADO**: Si `SaveChangesAsync` falla, retorna un error genérico

**Causas Probables:**

1. **Error de Base de Datos**: 
   - Constraint violations (claves foráneas, unique constraints)
   - Timeout de conexión
   - Transacciones fallidas

2. **Validaciones de Negocio**:
   - Categorías o productos que no existen (en ofertas)
   - Datos requeridos faltantes

3. **Problemas con UnitOfWork**:
   - `SaveChangesAsync` puede fallar por múltiples razones

**Soluciones Recomendadas:**

### Solución 1: Mejorar Logging de Errores

```csharp
// En CreateSedeCommandHandler.cs
catch (Exception ex)
{
    _logger.LogError(ex, 
        "Excepción al crear sede. Nombre: {Nombre}, Direccion: {Direccion}, Ciudad: {Ciudad}, " +
        "StackTrace: {StackTrace}, InnerException: {InnerException}",
        request.Sede.Nombre, 
        request.Sede.Direccion, 
        request.Sede.Ciudad,
        ex.StackTrace,
        ex.InnerException?.Message);
    
    return Result<SedeDto>.Failure($"Error al crear la sede: {ex.Message}");
}
```

### Solución 2: Validar Datos Antes de Guardar

```csharp
// En CreateSedeCommandHandler.cs - Agregar validaciones adicionales
public async Task<Result<SedeDto>> Handle(CreateSedeCommand request, CancellationToken cancellationToken)
{
    try
    {
        // Validar que no exista una sede con el mismo nombre
        var sedeExistente = await _unitOfWork.Sedes
            .FirstOrDefaultAsync(s => s.Nombre == request.Sede.Nombre, cancellationToken);
        
        if (sedeExistente != null)
        {
            return Result<SedeDto>.Failure("Ya existe una sede con ese nombre");
        }
        
        // ... resto del código
    }
}
```

### Solución 3: Revisar Logs del Servidor

**Acción Inmediata**: Revisar los logs en `src/Minimarket.API/logs/` para encontrar el stack trace específico del error 500.

```bash
# Buscar errores recientes en los logs
grep -i "error\|exception\|500" src/Minimarket.API/logs/minimarket-*.txt | tail -50
```

---

## 🔍 Problema 3: Errores 404 Not Found en Imágenes

### Validación ✅

**Problema Reportado:**
- `Archivo subido exitosamente. URL: https://minimarket.edvio.app/uploads/...`
- `/...png:1 Failed to load resource: the server responded with a status of **404** ()`
- **Síntoma**: "No lee las imagenes subidas ni las url, se ponen en imagen sin vista previa"

**Análisis del Código:**

1. **FileStorageService** (`FileStorageService.cs`):
   - ✅ Guarda archivos en `wwwroot/uploads/{folder}/`
   - ✅ Genera URLs usando `GetFileUrl()` que construye URLs absolutas
   - ⚠️ **PROBLEMA DETECTADO**: La URL generada puede no coincidir con la ruta del servidor web

2. **Program.cs - Configuración de Archivos Estáticos**:
   - ✅ Configurado para servir archivos desde `wwwroot/uploads` con ruta `/uploads`
   - ✅ Tiene `OnPrepareResponse` para agregar headers CORS
   - ⚠️ **PROBLEMA POTENCIAL**: En producción, si hay un proxy reverso (Nginx, IIS), la ruta puede no coincidir

3. **GetFileUrl()** (`FileStorageService.cs`):
   - ✅ Intenta obtener la URL base del contexto HTTP
   - ✅ Usa `BaseUrl` de configuración como fallback
   - ⚠️ **PROBLEMA DETECTADO**: En producción, `BaseUrl` podría estar vacío o incorrecto

**Causas Probables:**

1. **BaseUrl No Configurado en Producción**:
   - `appsettings.Production.json` tiene `BaseUrl: ""` (vacío)
   - El servicio genera URLs con `localhost:5000` como fallback

2. **Ruta del Servidor Web**:
   - Si hay un proxy reverso (Nginx), la ruta `/uploads/` debe estar configurada correctamente
   - Los archivos podrían estar en una ubicación diferente a la esperada

3. **Permisos de Archivos**:
   - Los archivos subidos podrían no tener permisos de lectura para el servidor web

**Soluciones Recomendadas:**

### Solución 1: Configurar BaseUrl en Producción

```json
// En appsettings.Production.json
{
  "BaseUrl": "https://minimarket.edvio.app",
  "FileStorage": {
    "BaseUrl": "https://minimarket.edvio.app"
  }
}
```

### Solución 2: Verificar Configuración de Nginx (Si aplica)

```nginx
# En nginx.conf o configuración del servidor
location /uploads/ {
    alias /ruta/completa/a/wwwroot/uploads/;
    expires 1y;
    add_header Cache-Control "public, immutable";
    access_log off;
}
```

### Solución 3: Verificar Permisos de Archivos

```bash
# En el servidor, verificar permisos
ls -la wwwroot/uploads/
# Debe mostrar permisos de lectura para el usuario del servidor web
```

### Solución 4: Mejorar GetFileUrl para Producción

```csharp
// En FileStorageService.cs - Mejorar GetFileUrl
public string GetFileUrl(string filePath)
{
    if (string.IsNullOrEmpty(filePath))
        return string.Empty;

    if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
        return filePath;

    var normalizedPath = filePath.Replace("\\", "/").TrimStart('/');

    // En producción, usar siempre la URL configurada
    string baseUrl = _baseUrl;
    
    // Si BaseUrl está vacío, intentar obtener del contexto HTTP
    if (string.IsNullOrEmpty(baseUrl) && _httpContextAccessor?.HttpContext != null)
    {
        var request = _httpContextAccessor.HttpContext.Request;
        var scheme = request.Scheme;
        
        // Forzar HTTPS en producción
        if (!request.Host.Host.Contains("localhost"))
        {
            scheme = "https";
        }
        
        baseUrl = $"{scheme}://{request.Host}";
    }
    
    // Si aún está vacío, usar un valor por defecto basado en el entorno
    if (string.IsNullOrEmpty(baseUrl))
    {
        // En producción, esto debería estar configurado
        _logger.LogWarning("BaseUrl no está configurado. Usando valor por defecto.");
        baseUrl = "https://minimarket.edvio.app"; // Ajustar según el dominio real
    }
    
    var url = $"{baseUrl.TrimEnd('/')}/{normalizedPath}";
    _logger.LogInformation("Generando URL para archivo: {FilePath} -> {Url}", filePath, url);
    return url;
}
```

### Solución 5: Verificar que los Archivos se Estén Guardando Correctamente

```csharp
// En FilesController.cs - Agregar validación después de guardar
var filePath = await _fileStorageService.SaveFileAsync(stream, file.FileName, normalizedFolder);
var fileUrl = _fileStorageService.GetFileUrl(filePath);

// Verificar que el archivo existe físicamente
var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
if (!File.Exists(physicalPath))
{
    _logger.LogError("Archivo no encontrado después de guardar: {PhysicalPath}", physicalPath);
    return StatusCode(500, new { error = "Error al guardar el archivo" });
}

_logger.LogInformation("Archivo guardado y verificado. PhysicalPath: {PhysicalPath}, FileUrl: {FileUrl}", 
    physicalPath, fileUrl);
```

---

## 🔍 Problema 4: Errores NG0203 de Angular

### Validación ✅

**Problema Reportado:**
- `ERROR M: NG0203`

**Análisis:**

El error `NG0203` en Angular generalmente indica:
- Problemas con Change Detection
- Acceso a propiedades `undefined` durante la inicialización
- Problemas con signals o computed values

**Causas Probables:**

1. **Datos No Disponibles Durante OnInit**:
   - Los componentes intentan acceder a datos de API que aún no han llegado
   - Las propiedades están `undefined` cuando el template intenta renderizarlas

2. **Problemas con Signals**:
   - El proyecto usa Angular Signals (`signal()`)
   - Si un signal se accede antes de inicializarse, puede causar NG0203

**Soluciones Recomendadas:**

### Solución 1: Usar Safe Navigation Operator

```html
<!-- En los templates -->
<img [src]="imageUrl()?.url || '/assets/placeholder.png'" alt="Imagen" />
<div *ngIf="sede()">{{ sede()?.nombre }}</div>
```

### Solución 2: Inicializar Signals con Valores por Defecto

```typescript
// En los componentes
sede = signal<Sede | null>(null);
imageUrl = signal<string | null>(null);

// En lugar de
sede = signal<Sede>(undefined); // ❌ Esto puede causar NG0203
```

### Solución 3: Verificar que los Datos Estén Disponibles

```typescript
// En los componentes
ngOnInit(): void {
  // Cargar datos primero
  this.loadSedes().subscribe({
    next: (sedes) => {
      this.sedes.set(sedes); // Solo establecer después de recibir datos
    },
    error: (error) => {
      console.error('Error loading sedes:', error);
      this.sedes.set([]); // Establecer array vacío en caso de error
    }
  });
}
```

---

## 📋 Plan de Acción Recomendado

### Prioridad Alta (Crítico)

1. **Configurar BaseUrl en Producción**
   - Editar `appsettings.Production.json` con la URL correcta
   - Verificar que `FileStorage:BaseUrl` esté configurado

2. **Revisar Logs del Servidor para Errores 500**
   - Buscar stack traces específicos en `src/Minimarket.API/logs/`
   - Identificar la causa raíz de los errores 500 en sedes y ofertas

3. **Verificar Permisos de Archivos en el Servidor**
   - Asegurar que `wwwroot/uploads/` tenga permisos de lectura
   - Verificar que el servidor web pueda acceder a los archivos

### Prioridad Media

4. **Implementar Refresh Token**
   - Reducir errores 401 por tokens expirados
   - Mejorar experiencia de usuario

5. **Mejorar Manejo de Errores en Handlers**
   - Agregar logging más detallado
   - Validar datos antes de guardar

### Prioridad Baja

6. **Corregir Errores NG0203**
   - Revisar templates y componentes
   - Asegurar inicialización correcta de signals

---

## 🔧 Comandos Útiles para Diagnóstico

### Revisar Logs del Servidor

```bash
# Ver errores recientes
tail -100 src/Minimarket.API/logs/minimarket-*.txt | grep -i "error\|exception\|500"

# Buscar errores específicos de sedes
grep -i "sede\|create.*sede" src/Minimarket.API/logs/minimarket-*.txt | grep -i "error\|exception"

# Buscar errores específicos de ofertas
grep -i "oferta\|create.*oferta" src/Minimarket.API/logs/minimarket-*.txt | grep -i "error\|exception"
```

### Verificar Archivos Subidos

```bash
# En el servidor, verificar que los archivos existen
ls -la src/Minimarket.API/wwwroot/uploads/sedes/
ls -la src/Minimarket.API/wwwroot/uploads/ofertas/
```

### Verificar Configuración

```bash
# Verificar que BaseUrl esté configurado
grep -i "baseurl\|fileStorage" src/Minimarket.API/appsettings*.json
```

---

## 📝 Notas Adicionales

1. **Entorno de Producción**: El proyecto parece estar usando `https://minimarket.edvio.app` como dominio de producción. Asegurar que todas las configuraciones apunten a este dominio.

2. **Proxy Reverso**: Si hay un proxy reverso (Nginx, IIS, etc.), verificar que las rutas `/uploads/` estén correctamente configuradas.

3. **CORS**: Los logs muestran algunos problemas de CORS con ngrok. Verificar la configuración de CORS en producción.

4. **Base de Datos**: Los errores 500 podrían estar relacionados con problemas de conexión a la base de datos. Verificar la cadena de conexión en producción.

---

## ✅ Conclusión

Los problemas identificados son **válidos y tienen causas técnicas específicas** que pueden ser resueltas siguiendo las soluciones propuestas. El problema más crítico es la configuración de `BaseUrl` en producción para las imágenes, seguido de los errores 500 que requieren revisión de logs del servidor para identificar la causa exacta.

