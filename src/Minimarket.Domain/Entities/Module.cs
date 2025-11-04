namespace Minimarket.Domain.Entities;

public class Module : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // Ej: "configuracion_edicion", "sedes", "productos"
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

