using MediatR;
using Microsoft.AspNetCore.Identity;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Permissions.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Permissions.Commands;

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Result<IEnumerable<RolePermissionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UpdateRolePermissionsCommandHandler(
        IUnitOfWork unitOfWork,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
    }

    public async Task<Result<IEnumerable<RolePermissionDto>>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var roleId = request.RolePermissions.RoleId;

        // Verificar que el rol existe
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return Result<IEnumerable<RolePermissionDto>>.Failure($"Rol con ID {roleId} no encontrado");
        }

        // Obtener permisos existentes para este rol
        var existingPermissions = (await _unitOfWork.RolePermissions.GetAllAsync(cancellationToken))
            .Where(rp => rp.RoleId == roleId)
            .ToList();

        var allModules = await _unitOfWork.Modules.GetAllAsync(cancellationToken);
        var modulesDict = allModules.ToDictionary(m => m.Id, m => m);

        // Eliminar permisos que ya no están en la lista
        var newModuleIds = request.RolePermissions.ModulePermissions.Select(mp => mp.ModuleId).ToHashSet();
        var permissionsToDelete = existingPermissions.Where(ep => !newModuleIds.Contains(ep.ModuleId)).ToList();

        foreach (var permission in permissionsToDelete)
        {
            await _unitOfWork.RolePermissions.DeleteAsync(permission, cancellationToken);
        }

        // Actualizar o crear permisos
        foreach (var modulePermission in request.RolePermissions.ModulePermissions)
        {
            if (!modulesDict.TryGetValue(modulePermission.ModuleId, out var module))
            {
                continue; // Saltar módulos que no existen
            }

            var existingPermission = existingPermissions.FirstOrDefault(ep => ep.ModuleId == modulePermission.ModuleId);

            if (existingPermission != null)
            {
                // Actualizar permiso existente
                existingPermission.CanView = modulePermission.CanView;
                existingPermission.CanCreate = modulePermission.CanCreate;
                existingPermission.CanEdit = modulePermission.CanEdit;
                existingPermission.CanDelete = modulePermission.CanDelete;

                await _unitOfWork.RolePermissions.UpdateAsync(existingPermission, cancellationToken);
            }
            else
            {
                // Crear nuevo permiso
                var newPermission = new RolePermission
                {
                    RoleId = roleId,
                    ModuleId = modulePermission.ModuleId,
                    CanView = modulePermission.CanView,
                    CanCreate = modulePermission.CanCreate,
                    CanEdit = modulePermission.CanEdit,
                    CanDelete = modulePermission.CanDelete
                };

                await _unitOfWork.RolePermissions.AddAsync(newPermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Retornar todos los permisos actualizados
        var query = new Queries.GetRolePermissionsQuery { RoleId = roleId };
        var queryHandler = new Queries.GetRolePermissionsQueryHandler(_unitOfWork, _roleManager);
        var result = await queryHandler.Handle(query, cancellationToken);

        return result;
    }
}

