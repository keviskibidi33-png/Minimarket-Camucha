using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Permissions.DTOs;

namespace Minimarket.Application.Features.Permissions.Queries;

public class GetRolePermissionsQuery : IRequest<Result<IEnumerable<RolePermissionDto>>>
{
    public Guid? RoleId { get; set; } // Opcional: filtrar por rol específico
}

