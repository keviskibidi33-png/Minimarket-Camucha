namespace Minimarket.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; } // FK a IdentityRole
    public Guid ModuleId { get; set; }
    public bool CanView { get; set; } = false;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;

    // Navigation properties
    public virtual Module Module { get; set; } = null!;
}

