namespace Minimarket.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } // Imagen de la categoría para la tienda
    public string? IconoUrl { get; set; } // Icono de la categoría
    public int Orden { get; set; } = 0; // Orden de visualización
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

