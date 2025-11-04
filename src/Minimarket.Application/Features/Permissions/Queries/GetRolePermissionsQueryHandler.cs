using MediatR;
using Microsoft.AspNetCore.Identity;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Permissions.DTOs;
using Minimarket.Domain.Interfaces;
using System.Security.Claims;

namespace Minimarket.Application.Features.Permissions.Queries;

public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, Result<IEnumerable<RolePermissionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public GetRolePermissionsQueryHandler(
        IUnitOfWork unitOfWork,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
    }

    public async Task<Result<IEnumerable<RolePermissionDto>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var allPermissions = await _unitOfWork.RolePermissions.GetAllAsync(cancellationToken);
        var allModules = await _unitOfWork.Modules.GetAllAsync(cancellationToken);
        var allRoles = _roleManager.Roles.ToList();

        var modulesDict = allModules.ToDictionary(m => m.Id, m => new { m.Nombre, m.Slug });
        var rolesDict = allRoles.ToDictionary(r => r.Id, r => r.Name ?? string.Empty);

        var filteredPermissions = request.RoleId.HasValue
            ? allPermissions.Where(rp => rp.RoleId == request.RoleId.Value)
            : allPermissions;

        var result = filteredPermissions
            .Select(rp =>
            {
                var moduleInfo = modulesDict.TryGetValue(rp.ModuleId, out var module) ? module : null;
                var roleName = rolesDict.TryGetValue(rp.RoleId, out var name) ? name : string.Empty;

                return new RolePermissionDto
                {
                    Id = rp.Id,
                    RoleId = rp.RoleId,
                    RoleName = roleName,
                    ModuleId = rp.ModuleId,
                    ModuleName = moduleInfo?.Nombre ?? string.Empty,
                    ModuleSlug = moduleInfo?.Slug ?? string.Empty,
                    CanView = rp.CanView,
                    CanCreate = rp.CanCreate,
                    CanEdit = rp.CanEdit,
                    CanDelete = rp.CanDelete
                };
            })
            .OrderBy(rp => rp.RoleName)
            .ThenBy(rp => rp.ModuleName)
            .ToList();

        return Result<IEnumerable<RolePermissionDto>>.Success(result);
    }
}

