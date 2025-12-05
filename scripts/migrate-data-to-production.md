# Guía de Migración de Datos Local → Producción

Esta guía te ayudará a migrar datos de tu base de datos local a producción en Coolify.

## Opción 1: Migración Manual usando SQL Server Management Studio (SSMS)

### Requisitos Previos
- SQL Server Management Studio (SSMS) instalado
- Acceso a la base de datos local
- Acceso a la base de datos de producción (a través de Coolify)

### Pasos

1. **Conectar a la base de datos local**
   - Abre SSMS
   - Conecta a: `localhost` o `(localdb)\MSSQLLocalDB`
   - Base de datos: `MinimarketDB`

2. **Exportar datos de tablas específicas**
   - Ejecuta estos scripts SQL para exportar datos:

```sql
-- Exportar Sedes
SELECT * FROM Sedes
-- Copia los resultados y guárdalos

-- Exportar Ofertas
SELECT * FROM Ofertas
-- Copia los resultados y guárdalos

-- Exportar Productos (si quieres migrar productos también)
SELECT * FROM Products
-- Copia los resultados y guárdalos

-- Exportar Categorías (si quieres migrar categorías también)
SELECT * FROM Categories
-- Copia los resultados y guárdalos
```

3. **Conectar a la base de datos de producción**
   - En Coolify, ve a tu aplicación
   - Obtén la cadena de conexión de la base de datos
   - Conecta usando SSMS con:
     - Servidor: `103.138.188.233` (o el IP de tu VPS)
     - Puerto: `1433` (si está expuesto) o usa el túnel SSH de Coolify
     - Usuario: `SA`
     - Contraseña: La que configuraste en `DB_PASSWORD`

4. **Importar datos**
   - Ejecuta scripts INSERT basados en los datos exportados
   - Asegúrate de ajustar los IDs si es necesario

## Opción 2: Usar la API para Crear Datos

### Ventajas
- Más seguro (usa la validación de la API)
- Mantiene relaciones correctas
- No requiere acceso directo a la base de datos

### Pasos

1. **Obtener datos de la API local**
```bash
# Obtener sedes
curl http://localhost:5000/api/sedes > sedes.json

# Obtener ofertas
curl http://localhost:5000/api/ofertas > ofertas.json
```

2. **Crear datos en producción usando la API**
```bash
# Autenticarse primero
curl -X POST https://minimarket.edvio.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@minimarketcamucha.com","password":"Admin123!"}' \
  > auth.json

# Extraer el token
TOKEN=$(cat auth.json | jq -r '.token')

# Crear sedes (una por una)
# Edita sedes.json y crea cada sede usando:
curl -X POST https://minimarket.edvio.app/api/sedes \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "sede": {
      "nombre": "Nombre de la Sede",
      "direccion": "Dirección",
      "ciudad": "Ciudad",
      "telefono": "Teléfono",
      "activa": true
    }
  }'

# Crear ofertas (una por una)
# Similar para ofertas...
```

## Opción 3: Script PowerShell para Migración Automática

Crea un archivo `migrate-data.ps1`:

```powershell
# Configuración
$LOCAL_API = "http://localhost:5000"
$PROD_API = "https://minimarket.edvio.app"
$ADMIN_EMAIL = "admin@minimarketcamucha.com"
$ADMIN_PASSWORD = "Admin123!"

# Autenticarse en producción
$authBody = @{
    email = $ADMIN_EMAIL
    password = $ADMIN_PASSWORD
} | ConvertTo-Json

$authResponse = Invoke-RestMethod -Uri "$PROD_API/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $authBody

$token = $authResponse.token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Obtener sedes de local
$sedes = Invoke-RestMethod -Uri "$LOCAL_API/api/sedes"

# Crear sedes en producción
foreach ($sede in $sedes) {
    $sedeBody = @{
        sede = @{
            nombre = $sede.nombre
            direccion = $sede.direccion
            ciudad = $sede.ciudad
            telefono = $sede.telefono
            activa = $sede.activa
        }
    } | ConvertTo-Json -Depth 10
    
    try {
        Invoke-RestMethod -Uri "$PROD_API/api/sedes" `
            -Method POST `
            -Headers $headers `
            -Body $sedeBody
        Write-Host "Sede creada: $($sede.nombre)" -ForegroundColor Green
    } catch {
        Write-Host "Error al crear sede $($sede.nombre): $_" -ForegroundColor Red
    }
}

# Similar para ofertas...
```

## Opción 4: Migración de Archivos (Imágenes)

Si tienes imágenes subidas localmente, necesitas copiarlas al volumen de Docker en producción:

1. **En local, exportar imágenes**
```bash
# Copiar todas las imágenes
tar -czf uploads-backup.tar.gz src/Minimarket.API/wwwroot/uploads/
```

2. **En producción (a través de Coolify)**
```bash
# Conectarte al contenedor de la API
docker exec -it <container-id> bash

# O usar Coolify para copiar archivos al volumen
# En Coolify, ve a Volumes > uploads > Upload files
```

## Notas Importantes

1. **IDs**: Los IDs (GUIDs) pueden cambiar si los generas nuevamente. Si necesitas mantener los mismos IDs, usa la Opción 1.

2. **Relaciones**: Asegúrate de migrar primero las entidades base (categorías, sedes) antes de las que dependen de ellas (productos, ofertas).

3. **Imágenes**: Las URLs de las imágenes cambiarán. Asegúrate de actualizar las referencias en la base de datos después de migrar las imágenes.

4. **Backup**: Siempre haz un backup de la base de datos de producción antes de migrar datos.

## Verificación Post-Migración

1. Verifica que las sedes se crearon:
```bash
curl https://minimarket.edvio.app/api/sedes
```

2. Verifica que las ofertas se crearon:
```bash
curl https://minimarket.edvio.app/api/ofertas
```

3. Verifica que las imágenes se cargan correctamente:
   - Abre el navegador y visita: `https://minimarket.edvio.app/uploads/products/[nombre-imagen].jpg`

