using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimarket.Domain.Interfaces;
using System.Security.Claims;

namespace Minimarket.Application.Common.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionAuthorizationHandler(
        UserManager<IdentityUser<Guid>> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Si el usuario no está autenticado, fallar
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        // Obtener el ID del usuario desde los claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        // Obtener el usuario y sus roles
        var user = await _userManager.FindByIdAsync(userIdClaim);
        if (user == null)
        {
            return;
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        // Si el usuario es Administrador, siempre permitir
        if (userRoles.Contains("Administrador"))
        {
            context.Succeed(requirement);
            return;
        }

        // Buscar el módulo por slug
        var module = await _unitOfWork.Modules.FirstOrDefaultAsync(
            m => m.Slug == requirement.ModuleSlug && m.IsActive,
            CancellationToken.None);

        if (module == null)
        {
            return;
        }

        // Obtener los roles del usuario como GUIDs
        var roleIds = new List<Guid>();
        foreach (var roleName in userRoles)
        {
            var identityRole = await _roleManager.FindByNameAsync(roleName);
            if (identityRole != null)
            {
                roleIds.Add(identityRole.Id);
            }
        }

        // Buscar permisos para los roles del usuario en este módulo
        var permissions = await _unitOfWork.RolePermissions.FindAsync(
            rp => roleIds.Contains(rp.RoleId) && rp.ModuleId == module.Id,
            CancellationToken.None);

        // Verificar el permiso específico requerido
        foreach (var permission in permissions)
        {
            bool hasPermission = requirement.Permission switch
            {
                "View" => permission.CanView,
                "Create" => permission.CanCreate,
                "Edit" => permission.CanEdit,
                "Delete" => permission.CanDelete,
                _ => false
            };

            if (hasPermission)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string ModuleSlug { get; }
    public string Permission { get; }

    public PermissionRequirement(string moduleSlug, string permission)
    {
        ModuleSlug = moduleSlug;
        Permission = permission;
    }
}

