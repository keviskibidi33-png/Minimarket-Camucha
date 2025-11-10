# Sistema de Permisos Granulares

## Estado Actual

**IMPORTANTE**: Actualmente el sistema **NO está usando permisos granulares** para autorizar acciones. Solo se está usando autorización simple por roles (`[Authorize(Roles = "Administrador")]`).

## Infraestructura Existente

El sistema tiene la infraestructura para permisos granulares:

1. **Entidad `RolePermission`**: Almacena permisos por rol y módulo
   - `CanView`: Ver módulo
   - `CanCreate`: Crear en módulo
   - `CanEdit`: Editar en módulo
   - `CanDelete`: Eliminar en módulo

2. **Entidad `Module`**: Define módulos del sistema con `Slug` único

3. **Controlador de Permisos**: `/api/permissions` para gestionar permisos

## Implementación de Permisos Granulares

Se ha creado la infraestructura para usar permisos granulares:

### 1. `RequirePermissionAttribute`
Atributo personalizado para requerir permisos específicos:
```csharp
[RequirePermission("usuarios", "View")]  // Ver usuarios
[RequirePermission("usuarios", "Create")] // Crear usuarios
[RequirePermission("usuarios", "Edit")]  // Editar usuarios
[RequirePermission("usuarios", "Delete")] // Eliminar usuarios
```

### 2. `PermissionAuthorizationHandler`
Handler que verifica permisos granulares:
- Si el usuario es "Administrador", siempre permite
- Busca el módulo por `Slug`
- Verifica si el rol del usuario tiene el permiso requerido

### 3. Configuración en `Program.cs`
- Handler registrado como servicio
- Sistema de autorización configurado

## Cómo Usar Permisos Granulares

### En Controladores

**ANTES (solo roles):**
```csharp
[Authorize(Roles = "Administrador")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllUsers() { ... }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser() { ... }
}
```

**DESPUÉS (permisos granulares):**
```csharp
[Authorize] // Requiere autenticación
public class UsersController : ControllerBase
{
    [HttpGet]
    [RequirePermission("usuarios", "View")]
    public async Task<IActionResult> GetAllUsers() { ... }
    
    [HttpPost]
    [RequirePermission("usuarios", "Create")]
    public async Task<IActionResult> CreateUser() { ... }
    
    [HttpPut("{id}")]
    [RequirePermission("usuarios", "Edit")]
    public async Task<IActionResult> UpdateUser() { ... }
    
    [HttpDelete("{id}")]
    [RequirePermission("usuarios", "Delete")]
    public async Task<IActionResult> DeleteUser() { ... }
}
```

## Módulos del Sistema

Los módulos deben estar registrados en la base de datos con sus `Slug` correspondientes:

- `usuarios` - Gestión de usuarios
- `productos` - Gestión de productos
- `ventas` - Gestión de ventas
- `pedidos` - Gestión de pedidos web
- `configuracion` - Configuración del sistema
- `sedes` - Gestión de sedes
- `categorias` - Gestión de categorías
- etc.

## Configuración de Permisos

Los permisos se configuran desde:
- **Frontend**: `/admin/configuraciones/permisos`
- **Backend**: `/api/permissions/role-permissions`

## Notas Importantes

1. **Administradores**: Siempre tienen todos los permisos (bypass)
2. **Módulos no encontrados**: Si un módulo no existe, se deniega el acceso
3. **Roles sin permisos**: Si un rol no tiene permisos configurados, se deniega el acceso
4. **Compatibilidad**: El sistema sigue funcionando con `[Authorize(Roles = "...")]` para mantener compatibilidad

## Próximos Pasos

Para activar completamente los permisos granulares:

1. ✅ Crear `RequirePermissionAttribute`
2. ✅ Crear `PermissionAuthorizationHandler`
3. ✅ Registrar handler en `Program.cs`
4. ⏳ Actualizar controladores para usar `[RequirePermission]`
5. ⏳ Crear módulos en la base de datos con sus `Slug`
6. ⏳ Configurar permisos iniciales para cada rol

