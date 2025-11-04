namespace Minimarket.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty; // Código de barras
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public Guid CategoryId { get; set; }
    public string? ImageUrl { get; set; } // URL de la imagen principal del producto
    public string ImagenesJson { get; set; } = "[]"; // JSON array de URLs de imágenes
    public string PaginasJson { get; set; } = "{}"; // JSON: {"home": true, "home_orden": 1, "categoria": true}
    public string SedesDisponiblesJson { get; set; } = "[]"; // JSON array de sede_ids
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
}

