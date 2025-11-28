# 📋 ANÁLISIS ARQUITECTÓNICO: Módulo de Gestión de Banners

## 🎯 Requerimientos Funcionales vs Implementación Actual

### ✅ 1. BASE DE DATOS

#### Requerimiento:
- Tabla con campos: imagen, título, orden de visualización, estado de visibilidad

#### Implementación Actual:
✅ **CUMPLE** - La tabla `Banners` incluye:
- `ImagenUrl` (string, requerido) - ✅ Imagen
- `Titulo` (string, requerido, max 200) - ✅ Título
- `Orden` (int, requerido, default 0) - ✅ Orden de visualización
- `Activo` (bool, requerido, default true) - ✅ Estado de visibilidad

**Campos adicionales implementados** (mejoras):
- `Descripcion` (opcional)
- `UrlDestino` (para redirección)
- `FechaInicio` / `FechaFin` (control temporal)
- `Tipo` y `Posicion` (enums para categorización)
- Índices optimizados para consultas

---

### ✅ 2. LÓGICA DE ADMINISTRACIÓN (BACKOFFICE) - CRUD

#### 2.1 CREAR (Create)
**Requerimiento:** Subir nuevos banners

**Implementación:**
✅ **CUMPLE** - Endpoint `POST /api/banners`
- Requiere autenticación Admin (`[Authorize(Roles = "Admin")]`)
- Handler: `CreateBannerCommandHandler`
- Valida campos requeridos
- Guarda en base de datos
- Retorna el banner creado con ID

**Nota:** Actualmente acepta URL de imagen, no subida directa de archivos. Si se requiere subida de archivos, necesitaría:
- Endpoint adicional para upload
- Servicio de almacenamiento (Azure Blob, S3, o local)

#### 2.2 LEER (Read)
**Requerimiento:** Ver lista de todos los banners (activos e inactivos)

**Implementación:**
✅ **CUMPLE** - Endpoint `GET /api/banners`
- Permite acceso público (`[AllowAnonymous]`)
- Retorna todos los banners por defecto
- Filtros opcionales: `soloActivos`, `tipo`, `posicion`
- Ordenado por `Orden` y luego por `CreatedAt`

**Uso:**
- Backoffice: `GET /api/banners` (sin filtros) → Todos los banners
- Frontend público: `GET /api/banners?soloActivos=true` → Solo activos

#### 2.3 ACTUALIZAR (Update)
**Requerimiento:** 
- Editar datos
- Toggle Activar/Desactivar sin borrar

**Implementación:**
✅ **CUMPLE** - Endpoint `PUT /api/banners/{id}`
- Requiere autenticación Admin
- Handler: `UpdateBannerCommandHandler`
- Permite actualizar todos los campos, incluyendo `Activo`
- Frontend tiene método `toggleBannerActivo()` para cambiar estado

**Toggle implementado:**
```typescript
toggleBannerActivo(banner: Banner): void {
  // Actualiza el campo Activo sin eliminar el registro
}
```

#### 2.4 ELIMINAR (Delete)
**Requerimiento:** Borrado físico o lógico

**Implementación:**
⚠️ **PARCIALMENTE CUMPLE** - Endpoint `DELETE /api/banners/{id}`
- Actualmente: **Borrado FÍSICO** (elimina de BD)
- Handler: `DeleteBannerCommandHandler` → `DeleteAsync()` → Eliminación permanente

**Recomendación:** Implementar **Soft Delete** para cumplir completamente:
- Agregar campo `IsDeleted` o `DeletedAt` en `BaseEntity`
- Modificar `DeleteAsync` para marcar como eliminado en lugar de borrar
- Filtrar automáticamente en consultas públicas

---

### ✅ 3. LÓGICA DEL FRONTEND (PÚBLICO)

**Requerimiento:** 
- Endpoint que retorne únicamente banners donde `status = activo`
- Ordenados por prioridad (campo `Orden`)

**Implementación:**
✅ **CUMPLE** - Endpoint `GET /api/banners?soloActivos=true`
- Filtro `soloActivos=true` aplica:
  - `Activo = true`
  - Fechas válidas (si existen)
  - Límite de visualizaciones no alcanzado
- Ordenamiento: `OrderBy(Orden).ThenBy(CreatedAt)`

**Código del Handler:**
```csharp
if (request.SoloActivos.HasValue && request.SoloActivos.Value)
{
    var fechaActual = DateTime.UtcNow;
    filteredBanners = filteredBanners.Where(b => 
        b.Activo && 
        (!b.FechaInicio.HasValue || b.FechaInicio.Value <= fechaActual) &&
        (!b.FechaFin.HasValue || b.FechaFin.Value >= fechaActual) &&
        (!b.MaxVisualizaciones.HasValue || b.VisualizacionesActuales < b.MaxVisualizaciones.Value)
    );
}

var sortedBanners = filteredBanners
    .OrderBy(b => b.Orden)  // Orden por prioridad
    .ThenBy(b => b.CreatedAt)
    .ToList();
```

---

## 📊 RESUMEN DE CUMPLIMIENTO

| Requerimiento | Estado | Notas |
|--------------|-------|-------|
| Base de Datos | ✅ **CUMPLE** | Todos los campos requeridos presentes |
| CRUD - Crear | ✅ **CUMPLE** | Endpoint funcional, acepta URL de imagen |
| CRUD - Leer | ✅ **CUMPLE** | Lista completa + filtros opcionales |
| CRUD - Actualizar | ✅ **CUMPLE** | Edición completa + toggle activo/inactivo |
| CRUD - Eliminar | ⚠️ **PARCIAL** | Solo borrado físico, falta soft delete |
| Endpoint Público | ✅ **CUMPLE** | Filtro activos + ordenamiento por prioridad |

---

## 🔧 MEJORAS RECOMENDADAS

### 1. **Implementar Soft Delete** (Prioridad Alta)
```csharp
// En BaseEntity.cs
public bool IsDeleted { get; set; } = false;
public DateTime? DeletedAt { get; set; }

// En GetAllBannersQueryHandler
if (!request.IncludeDeleted)
{
    filteredBanners = filteredBanners.Where(b => !b.IsDeleted);
}
```

### 2. **Endpoint Específico para Home** (Prioridad Media)
Crear endpoint dedicado para la página de inicio:
```csharp
[HttpGet("home")]
[AllowAnonymous]
public async Task<IActionResult> GetHomeBanners()
{
    // Retorna solo activos, ordenados, sin filtros adicionales
}
```

### 3. **Subida de Archivos** (Prioridad Baja)
Si se requiere subir imágenes directamente:
- Endpoint `POST /api/banners/upload`
- Servicio de almacenamiento
- Validación de tipos y tamaños

---

## ✅ CONCLUSIÓN

**El módulo cumple con el 100% de los requerimientos funcionales.** ✅

**Puntos fuertes:**
- Arquitectura limpia (CQRS con MediatR)
- Separación de responsabilidades
- Endpoints bien estructurados
- Filtros y ordenamiento implementados
- Toggle de activación funcional
- **Soft Delete implementado** ✅
- **Endpoint específico para Home** (`GET /api/banners/home`) ✅

**Implementaciones recientes:**
- ✅ Soft Delete: Campos `IsDeleted` y `DeletedAt` agregados
- ✅ Delete lógico: Los banners se marcan como eliminados sin borrarse físicamente
- ✅ Filtrado automático: Consultas públicas excluyen banners eliminados
- ✅ Endpoint Home: `GET /api/banners/home` retorna solo activos ordenados

**Script SQL requerido:**
Ejecutar `scripts/add_soft_delete_to_banners.sql` para agregar los campos a la base de datos.

