# Guía de Verificación: BaseUrl y Archivos Estáticos

## ✅ Verificación de BASE_URL

### 1. Verificar que la Variable de Entorno se Está Leyendo

El código lee `BaseUrl` desde la configuración en `FileStorageService.cs`:

```csharp
var configuredBaseUrl = _configuration["FileStorage:BaseUrl"] ?? _configuration["BaseUrl"];
```

**En Coolify/Docker**, las variables de entorno se mapean automáticamente:
- `BASE_URL` → `BaseUrl` en la configuración
- `FILE_STORAGE__BASE_URL` → `FileStorage:BaseUrl` en la configuración

### 2. Verificar en los Logs del Servidor

Busca en los logs del inicio del servicio:

```bash
# Buscar en los logs la inicialización de FileStorageService
grep -i "FileStorageService inicializado" src/Minimarket.API/logs/minimarket-*.txt | tail -5
```

Deberías ver algo como:
```
FileStorageService inicializado con BaseUrl: https://minimarket.edvio.app
```

Si ves `http://localhost:5000`, significa que **no está leyendo la variable de entorno**.

### 3. Verificar que la Variable de Entorno Esté Configurada en Coolify

En la imagen que compartiste, veo que tienes:
- ✅ `BASE_URL` = `https://minimarket.edvio.app`

**Asegúrate de que:**
1. La variable esté marcada como "Available at Runtime" ✅ (ya lo está)
2. El servicio se haya reiniciado después de configurar la variable
3. La variable esté en el servicio correcto (el servicio `api`)

---

## ✅ Verificación de Archivos Estáticos (wwwroot/uploads/)

### 1. Verificar Configuración en Program.cs

El código ya está configurado correctamente:

```330:363:src/Minimarket.API/Program.cs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "",
    OnPrepareResponse = ctx =>
    {
        // Permitir CORS para archivos estáticos (imágenes)
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        // Cache para archivos estáticos
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
    }
});

// Configurar ruta específica para /uploads/ con mejor manejo de errores
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Permitir CORS para archivos estáticos (imágenes)
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        // Cache para archivos estáticos
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
        // Log para debugging
        var logger = ctx.Context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Serving static file: {Path}", ctx.Context.Request.Path);
    },
    ServeUnknownFileTypes = true, // Permitir servir cualquier tipo de archivo
    DefaultContentType = "application/octet-stream" // Tipo por defecto si no se puede determinar
});
```

✅ **Esto está correcto**. Los archivos en `wwwroot/uploads/` deberían ser accesibles en `/uploads/`.

### 2. Verificar Permisos en el Contenedor Docker

Si estás usando Docker/Coolify, los permisos se manejan automáticamente, pero puedes verificar:

**Opción A: Desde Coolify (si tienes acceso SSH)**
```bash
# Conectarse al contenedor
docker exec -it <container-id> ls -la /app/wwwroot/uploads/

# Deberías ver algo como:
# drwxr-xr-x 2 root root 4096 Dec  3 16:00 sedes
# drwxr-xr-x 2 root root 4096 Dec  3 16:00 products
# -rw-r--r-- 1 root root 12345 Dec  3 16:00 imagen.png
```

**Opción B: Verificar desde los Logs**
Busca en los logs cuando se crean los directorios:

```bash
grep -i "Directorio de uploads creado\|Directorio de carpeta creado" src/Minimarket.API/logs/minimarket-*.txt
```

### 3. Verificar que los Archivos se Estén Guardando

Busca en los logs cuando se suben archivos:

```bash
grep -i "Archivo guardado\|Archivo guardado exitosamente" src/Minimarket.API/logs/minimarket-*.txt | tail -10
```

Deberías ver algo como:
```
Archivo guardado: /app/wwwroot/uploads/sedes/abc123-def456-ghi789.png
Archivo guardado exitosamente. FilePath: uploads/sedes/abc123.png, FileUrl: https://minimarket.edvio.app/uploads/sedes/abc123.png
```

---

## 🔍 Diagnóstico: ¿Por qué las Imágenes Dan 404?

### Posibles Causas:

1. **BaseUrl no se está leyendo correctamente**
   - **Solución**: Verificar logs de inicio del servicio
   - **Verificar**: Que la variable `BASE_URL` esté en el servicio `api` en Coolify

2. **Ruta del archivo no coincide con la URL generada**
   - **Solución**: Verificar que `GetFileUrl()` genere URLs correctas
   - **Verificar**: Comparar la URL generada con la ruta física del archivo

3. **Proxy Reverso (Nginx) no está configurado**
   - **Solución**: Si hay un proxy reverso, asegurar que `/uploads/` esté configurado
   - **Verificar**: En Coolify, verificar la configuración del proxy

4. **Volumen Docker no está montado correctamente**
   - **Solución**: Verificar que el volumen `uploads:/app/wwwroot/uploads` esté montado
   - **Verificar**: En Coolify, verificar los volúmenes del servicio

---

## 🧪 Pruebas para Verificar

### Prueba 1: Verificar que BaseUrl se Está Leyendo

Agrega un endpoint temporal para verificar:

```csharp
// En cualquier Controller (temporal, solo para debugging)
[HttpGet("debug/config")]
[AllowAnonymous]
public IActionResult GetConfig()
{
    return Ok(new
    {
        BaseUrl = _configuration["BaseUrl"],
        FileStorageBaseUrl = _configuration["FileStorage:BaseUrl"],
        Environment = _configuration["ASPNETCORE_ENVIRONMENT"]
    });
}
```

Luego accede a: `https://minimarket.edvio.app/api/debug/config`

Deberías ver:
```json
{
  "baseUrl": "https://minimarket.edvio.app",
  "fileStorageBaseUrl": null,
  "environment": "Production"
}
```

### Prueba 2: Verificar que los Archivos Estáticos se Sirven

1. Sube una imagen desde la aplicación
2. Copia la URL que se genera (ej: `https://minimarket.edvio.app/uploads/sedes/abc123.png`)
3. Abre esa URL directamente en el navegador
4. Si da 404, verifica:
   - Que el archivo existe físicamente en el servidor
   - Que la ruta en la URL coincide con la ruta física
   - Que el servidor web tiene permisos para leer el archivo

### Prueba 3: Verificar Logs de Archivos Estáticos

Busca en los logs cuando intentas acceder a una imagen:

```bash
grep -i "Serving static file" src/Minimarket.API/logs/minimarket-*.txt | tail -10
```

Si no ves estos logs cuando intentas acceder a una imagen, significa que la petición no está llegando al servidor de archivos estáticos (posible problema de proxy reverso).

---

## ✅ Checklist de Verificación

- [ ] Variable `BASE_URL` configurada en Coolify como `https://minimarket.edvio.app`
- [ ] Variable marcada como "Available at Runtime"
- [ ] Servicio reiniciado después de configurar la variable
- [ ] Logs muestran: `FileStorageService inicializado con BaseUrl: https://minimarket.edvio.app`
- [ ] Directorio `wwwroot/uploads/` existe y tiene subdirectorios
- [ ] Archivos se están guardando correctamente (verificar logs)
- [ ] URLs generadas tienen el formato: `https://minimarket.edvio.app/uploads/...`
- [ ] Archivos estáticos se sirven correctamente (probar accediendo directamente a una URL)

---

## 🚨 Si Aún Hay Problemas

1. **Verificar que el proxy reverso (si existe) esté configurado correctamente**
   - En Coolify, verificar la configuración de routing
   - Asegurar que `/uploads/` se enrute al servicio `api`

2. **Verificar que el volumen Docker esté montado**
   - En Coolify, verificar que el volumen `uploads:/app/wwwroot/uploads` esté configurado

3. **Revisar logs completos del servicio**
   - Buscar errores relacionados con archivos estáticos
   - Verificar que no haya errores de permisos

4. **Probar accediendo directamente al contenedor**
   - Verificar que los archivos existen físicamente
   - Verificar permisos de los archivos

