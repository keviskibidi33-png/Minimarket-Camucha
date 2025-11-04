namespace Minimarket.Application.Features.Permissions.DTOs;

public class RolePermissionDto
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleSlug { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

