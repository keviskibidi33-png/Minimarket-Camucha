# 🔧 CORRECCIÓN: Mostrar Todos los Banners en Backoffice

## 🐛 Problema Identificado

El módulo de administración de banners no mostraba todos los banners existentes porque:
1. El filtro de `IsDeleted` se aplicaba incluso en consultas del backoffice
2. Los banners existentes podrían tener `IsDeleted = NULL` si la columna no existía previamente

## ✅ Solución Implementada

### 1. **Corrección en GetAllBannersQueryHandler**

**Antes:**
```csharp
// Filtraba eliminados en TODAS las consultas
filteredBanners = filteredBanners.Where(b => !b.IsDeleted);
```

**Después:**
```csharp
// Solo filtra eliminados en consultas públicas (soloActivos=true)
// El backoffice ve TODOS los banners (activos, inactivos, eliminados)
if (request.SoloActivos.HasValue && request.SoloActivos.Value)
{
    // Consulta pública: excluir eliminados
    filteredBanners = filteredBanners.Where(b => !b.IsDeleted || b.IsDeleted == null);
    // ... filtros adicionales para activos
}
// Backoffice: NO filtrar por IsDeleted - mostrar TODOS
```

### 2. **Lógica de Filtrado**

| Contexto | Filtro IsDeleted | Filtro Activo | Resultado |
|----------|------------------|---------------|-----------|
| **Backoffice** (sin `soloActivos`) | ❌ NO filtra | ❌ NO filtra | Muestra TODOS (activos, inactivos, eliminados) |
| **Público** (`soloActivos=true`) | ✅ Filtra eliminados | ✅ Filtra solo activos | Solo activos no eliminados |

### 3. **Script SQL para Corregir Banners Existentes**

**Archivo:** `scripts/fix_existing_banners_soft_delete.sql`

Este script:
- Establece `IsDeleted = 0` en todos los banners existentes que tengan `NULL` o valor diferente
- Asegura que los banners existentes se muestren en el backoffice
- Muestra estadísticas de banners (activos, inactivos, eliminados)

## 📋 Pasos para Aplicar la Corrección

### Paso 1: Ejecutar Script SQL (si aún no se ejecutó)
```sql
-- Ejecutar: scripts/add_soft_delete_to_banners.sql
-- Esto agrega las columnas IsDeleted y DeletedAt
```

### Paso 2: Corregir Banners Existentes
```sql
-- Ejecutar: scripts/fix_existing_banners_soft_delete.sql
-- Esto asegura que todos los banners existentes tengan IsDeleted = 0
```

### Paso 3: Reiniciar Backend
```powershell
# Detener backend actual
# Reiniciar backend para aplicar cambios en código
cd D:\Documentos\Minimarket-Camucha\src\Minimarket.API
dotnet run
```

## ✅ Resultado Esperado

Después de aplicar la corrección:

1. **Backoffice (`GET /api/banners` sin parámetros):**
   - ✅ Muestra TODOS los banners (activos, inactivos, eliminados)
   - ✅ Permite gestionar todos los banners existentes
   - ✅ Los banners existentes aparecen correctamente

2. **Frontend Público (`GET /api/banners?soloActivos=true`):**
   - ✅ Solo muestra banners activos y no eliminados
   - ✅ Filtra correctamente por fechas y límites

3. **Home (`GET /api/banners/home`):**
   - ✅ Solo muestra banners activos y no eliminados
   - ✅ Ordenados por prioridad

## 🔍 Verificación

Para verificar que funciona:

1. **Backoffice:** Debe mostrar todos los banners existentes
2. **Frontend:** Solo debe mostrar banners activos
3. **Base de datos:** Todos los banners existentes deben tener `IsDeleted = 0`

## 📝 Notas Importantes

- Los banners eliminados (soft delete) **SÍ se muestran en el backoffice** para permitir su gestión
- Los banners eliminados **NO se muestran en consultas públicas**
- El campo `IsDeleted` maneja valores `NULL` como `false` para compatibilidad con banners existentes

