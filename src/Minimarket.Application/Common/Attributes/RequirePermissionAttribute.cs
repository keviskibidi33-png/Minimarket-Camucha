using Microsoft.AspNetCore.Authorization;
using Minimarket.Application.Common.Authorization;

namespace Minimarket.Application.Common.Attributes;

/// <summary>
/// Atributo para requerir un permiso granular específico en un módulo
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public string ModuleSlug { get; }
    public string Permission { get; } // "View", "Create", "Edit", "Delete"

    public RequirePermissionAttribute(string moduleSlug, string permission)
    {
        ModuleSlug = moduleSlug;
        Permission = permission;
        // Usar un policy name único que será manejado por el handler
        Policy = $"Permission:{moduleSlug}:{permission}";
    }
}

