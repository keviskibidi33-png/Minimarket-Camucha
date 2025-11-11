namespace Minimarket.Application.Features.Products.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<string> Imagenes { get; set; } = new();
    public Dictionary<string, object> Paginas { get; set; } = new();
    public List<Guid> SedesDisponibles { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpirationDate { get; set; } // Fecha de vencimiento
}

