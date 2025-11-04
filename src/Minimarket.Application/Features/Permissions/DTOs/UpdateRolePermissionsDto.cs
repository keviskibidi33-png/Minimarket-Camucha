namespace Minimarket.Application.Features.Permissions.DTOs;

public class UpdateRolePermissionsDto
{
    public Guid RoleId { get; set; }
    public List<ModulePermissionDto> ModulePermissions { get; set; } = new();
}

public class ModulePermissionDto
{
    public Guid ModuleId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

