using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Permissions.DTOs;

namespace Minimarket.Application.Features.Permissions.Commands;

public class UpdateRolePermissionsCommand : IRequest<Result<IEnumerable<RolePermissionDto>>>
{
    public UpdateRolePermissionsDto RolePermissions { get; set; } = new();
}

